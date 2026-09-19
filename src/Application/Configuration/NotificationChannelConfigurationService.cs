using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Entities;
using Persistence.Enums;

namespace Application.Configuration;

public class NotificationChannelConfigurationService : INotificationChannelConfigurationService
{
	private readonly NotificationContext _dbContext;
	private readonly ISecretProtector _secretProtector;

	public NotificationChannelConfigurationService(NotificationContext dbContext, ISecretProtector secretProtector)
	{
		_dbContext = dbContext;
		_secretProtector = secretProtector;
	}

	public async Task<IReadOnlyList<NotificationChannelSettingValue>> GetSettingsAsync(int channelId)
	{
		var settings = await _dbContext.ChannelSettings
			.AsNoTracking()
			.Where(s => s.ChannelId == channelId)
			.Include(s => s.SettingDefinition)
			.ToListAsync();

		var result = new List<NotificationChannelSettingValue>();

		foreach (var setting in settings)
		{
			var definition = setting.SettingDefinition ?? await _dbContext.SettingDefinitions.AsNoTracking().FirstOrDefaultAsync(d => d.Id == setting.SettingDefinitionId);
			if (definition == null) continue;

			var value = ReadSettingValue(setting, definition);
			var isSensitive = definition.IsSensitive;

			result.Add(new NotificationChannelSettingValue
			{
				Code = definition.Code,
				Name = definition.Name,
				DataType = definition.DataType,
				IsRequired = definition.IsRequired,
				IsSensitive = isSensitive,
				IsConfigured = value != null,
				Value = isSensitive ? null : value
			});
		}

		return result;
	}

	public async Task<NotificationChannelSettingValue?> GetSettingValueAsync(int channelId, string code)
	{
		var channel = await _dbContext.Channels
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == channelId);

		if (channel == null)
		{
			return null;
		}

		var definition = await _dbContext.SettingDefinitions
			.AsNoTracking()
			.FirstOrDefaultAsync(d => d.ChannelTypeId == channel.ChannelTypeId && d.Code == code);

		if (definition == null)
		{
			return null;
		}

		var setting = await _dbContext.ChannelSettings
			.AsNoTracking()
			.Include(s => s.SettingDefinition)
			.FirstOrDefaultAsync(s => s.ChannelId == channelId && s.SettingDefinitionId == definition.Id);

		if (setting == null)
		{
			return new NotificationChannelSettingValue
			{
				Code = definition.Code,
				Name = definition.Name,
				DataType = definition.DataType,
				IsRequired = definition.IsRequired,
				IsSensitive = definition.IsSensitive,
				IsConfigured = false,
				Value = null
			};
		}

		var value = ReadSettingValue(setting, definition);

		return new NotificationChannelSettingValue
		{
			Code = definition.Code,
			Name = definition.Name,
			DataType = definition.DataType,
			IsRequired = definition.IsRequired,
			IsSensitive = definition.IsSensitive,
			IsConfigured = value != null,
			Value = definition.IsSensitive ? null : value
		};
	}

	public async Task<string?> GetSecretAsync(int channelId, string code)
	{
		var channel = await _dbContext.Channels
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == channelId);

		if (channel == null)
		{
			return null;
		}

		var definition = await _dbContext.SettingDefinitions
			.AsNoTracking()
			.FirstOrDefaultAsync(d => d.ChannelTypeId == channel.ChannelTypeId && d.Code == code && d.IsSensitive && d.Enabled);

		if (definition == null)
		{
			return null;
		}

		var setting = await _dbContext.ChannelSettings
			.AsNoTracking()
			.Include(s => s.SettingDefinition)
			.FirstOrDefaultAsync(s => s.ChannelId == channelId && s.SettingDefinitionId == definition.Id);

		if (setting == null || string.IsNullOrWhiteSpace(setting.StringValue))
		{
			return null;
		}

		try
		{
			return _secretProtector.Decrypt(setting.StringValue);
		}
		catch
		{
			// Never treat an undecryptable value as plaintext.
			return null;
		}
	}

	public async Task<IReadOnlyList<string>> ValidateAsync(int channelId)
	{
		var channel = await _dbContext.Channels.AsNoTracking().FirstOrDefaultAsync(c => c.Id == channelId);
		if (channel == null)
		{
			return new[] { "El canal no existe." };
		}

		var definitions = await _dbContext.SettingDefinitions
			.AsNoTracking()
			.Where(d => d.ChannelTypeId == channel.ChannelTypeId && d.Enabled)
			.ToListAsync();
		var settings = await _dbContext.ChannelSettings
			.AsNoTracking()
			.Where(s => s.ChannelId == channelId)
			.ToListAsync();

		var errors = new List<string>();
		foreach (var definition in definitions)
		{
			var setting = settings.FirstOrDefault(s => s.SettingDefinitionId == definition.Id);
			if (setting == null || !HasValue(setting, definition.DataType))
			{
				if (definition.IsRequired)
				{
					errors.Add($"La configuración requerida '{definition.Code}' no está configurada.");
				}

				continue;
			}

			if (!HasValueInExpectedColumn(setting, definition.DataType))
			{
				errors.Add($"La configuración '{definition.Code}' tiene un tipo de dato inválido.");
			}
		}

		return errors;
	}

	public async Task SaveSettingAsync(int channelId, string code, object? value, bool isExplicitDelete = false)
	{
		var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
		if (channel == null)
		{
			throw new InvalidOperationException("El canal no existe.");
		}

		var definition = await _dbContext.SettingDefinitions
			.FirstOrDefaultAsync(d => d.ChannelTypeId == channel.ChannelTypeId && d.Code == code && d.Enabled);

		if (definition == null)
		{
			throw new InvalidOperationException($"La configuración '{code}' no existe para el tipo de canal.");
		}

		if (isExplicitDelete)
		{
			var existing = await _dbContext.ChannelSettings.FirstOrDefaultAsync(s => s.ChannelId == channelId && s.SettingDefinitionId == definition.Id);
			if (existing != null)
			{
				_dbContext.ChannelSettings.Remove(existing);
			}

			await _dbContext.SaveChangesAsync();
			return;
		}

		if (value == null)
		{
			return;
		}

		var normalized = NormalizeValue(value, definition.DataType, definition.IsSensitive);
		var setting = await _dbContext.ChannelSettings
			.FirstOrDefaultAsync(s => s.ChannelId == channelId && s.SettingDefinitionId == definition.Id);

		if (setting == null)
		{
			_dbContext.ChannelSettings.Add(new NotificationChannelSetting
			{
				ChannelId = channelId,
				SettingDefinitionId = definition.Id,
				StringValue = normalized.StringValue,
				IntValue = normalized.IntValue,
				BoolValue = normalized.BoolValue,
				DecimalValue = normalized.DecimalValue,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			});
		}
		else
		{
			setting.StringValue = normalized.StringValue;
			setting.IntValue = normalized.IntValue;
			setting.BoolValue = normalized.BoolValue;
			setting.DecimalValue = normalized.DecimalValue;
			setting.UpdatedAt = DateTime.UtcNow;
		}

		await _dbContext.SaveChangesAsync();
	}

	public async Task DeleteSettingAsync(int channelId, string code)
	{
		var channel = await _dbContext.Channels.AsNoTracking().FirstOrDefaultAsync(c => c.Id == channelId);
		if (channel == null)
		{
			return;
		}

		var definition = await _dbContext.SettingDefinitions.FirstOrDefaultAsync(d => d.ChannelTypeId == channel.ChannelTypeId && d.Code == code && d.Enabled);
		if (definition == null)
		{
			return;
		}

		var setting = await _dbContext.ChannelSettings.FirstOrDefaultAsync(s => s.ChannelId == channelId && s.SettingDefinitionId == definition.Id);
		if (setting != null)
		{
			_dbContext.ChannelSettings.Remove(setting);
			await _dbContext.SaveChangesAsync();
		}
	}

	private static object? ReadSettingValue(NotificationChannelSetting setting, NotificationSettingDefinition definition)
	{
		return definition.DataType switch
		{
			NotificationSettingDataType.String => definition.IsSensitive ? null : setting.StringValue,
			NotificationSettingDataType.Int => setting.IntValue,
			NotificationSettingDataType.Bool => setting.BoolValue,
			NotificationSettingDataType.Decimal => setting.DecimalValue,
			_ => null
		};
	}

	private (string? StringValue, int? IntValue, bool? BoolValue, decimal? DecimalValue) NormalizeValue(object? rawValue, NotificationSettingDataType dataType, bool isSensitive)
	{
		switch (dataType)
		{
			case NotificationSettingDataType.Int:
				if (!int.TryParse(rawValue?.ToString(), out var intValue))
					throw new InvalidOperationException("El valor no tiene el tipo INT esperado.");
				return (null, intValue, null, null);
			case NotificationSettingDataType.Bool:
				if (!bool.TryParse(rawValue?.ToString(), out var boolValue))
					throw new InvalidOperationException("El valor no tiene el tipo BOOL esperado.");
				return (null, null, boolValue, null);
			case NotificationSettingDataType.Decimal:
				if (!decimal.TryParse(rawValue?.ToString(), out var decimalValue))
					throw new InvalidOperationException("El valor no tiene el tipo DECIMAL esperado.");
				return (null, null, null, decimalValue);
			default:
				var stringValue = rawValue?.ToString() ?? string.Empty;
				var finalValue = isSensitive ? _secretProtector.Encrypt(stringValue) : stringValue;
				return (finalValue, null, null, null);
		}
	}

	private static bool HasValue(NotificationChannelSetting setting, NotificationSettingDataType dataType) => dataType switch
	{
		NotificationSettingDataType.String => !string.IsNullOrWhiteSpace(setting.StringValue),
		NotificationSettingDataType.Int => setting.IntValue.HasValue,
		NotificationSettingDataType.Bool => setting.BoolValue.HasValue,
		NotificationSettingDataType.Decimal => setting.DecimalValue.HasValue,
		_ => false
	};

	private static bool HasValueInExpectedColumn(NotificationChannelSetting setting, NotificationSettingDataType dataType) => dataType switch
	{
		NotificationSettingDataType.String => setting.StringValue != null && setting.IntValue == null && setting.BoolValue == null && setting.DecimalValue == null,
		NotificationSettingDataType.Int => setting.StringValue == null && setting.IntValue.HasValue && setting.BoolValue == null && setting.DecimalValue == null,
		NotificationSettingDataType.Bool => setting.StringValue == null && setting.IntValue == null && setting.BoolValue.HasValue && setting.DecimalValue == null,
		NotificationSettingDataType.Decimal => setting.StringValue == null && setting.IntValue == null && setting.BoolValue == null && setting.DecimalValue.HasValue,
		_ => false
	};
}
