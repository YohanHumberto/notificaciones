using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Security;

namespace NotificationService.Tests;

public class ChannelConfigurationSecurityTests
{
    [Fact]
    public void SecretProtector_EncryptsAndDecryptsValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NotificationSecurity:EncryptionKey"] = "0123456789ABCDEF0123456789ABCDEF"
            })
            .Build();

        var protector = new AesSecretProtector(configuration);
        var original = "super-secret-api-key";

        var encrypted = protector.Encrypt(original);

        Assert.NotEqual(original, encrypted);
        Assert.Equal(original, protector.Decrypt(encrypted));
    }

    [Fact]
    public async Task ChannelSettingsService_DoesNotExposeSensitiveValues()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NotificationSecurity:EncryptionKey"] = "0123456789ABCDEF0123456789ABCDEF"
            })
            .Build();

        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new NotificationDbContext(options);

        var channelType = new NotificationChannelType { Code = "EMAIL", Name = "Email", Enabled = true };
        dbContext.ChannelTypes.Add(channelType);
        await dbContext.SaveChangesAsync();

        var settingDef = new NotificationSettingDefinition
        {
            ChannelTypeId = channelType.Id,
            Code = "SMTP_PASSWORD",
            Name = "SMTP Password",
            DataType = NotificationSettingDataType.String,
            IsRequired = true,
            IsSensitive = true,
            Enabled = true
        };
        dbContext.SettingDefinitions.Add(settingDef);

        var channel = new NotificationChannel
        {
            Name = "SMTP Test",
            ChannelTypeId = channelType.Id,
            Type = ChannelType.Email,
            IsActive = true
        };
        dbContext.Channels.Add(channel);
        await dbContext.SaveChangesAsync();

        var service = new NotificationChannelConfigurationService(dbContext, new AesSecretProtector(configuration));
        await service.SaveSettingAsync(channel.Id, "SMTP_PASSWORD", "plain-text-password");

        var setting = await dbContext.ChannelSettings.SingleAsync();
        Assert.NotEqual("plain-text-password", setting.StringValue);

        var value = await service.GetSecretAsync(channel.Id, "SMTP_PASSWORD");
        Assert.Equal("plain-text-password", value);

        var secureValue = await service.GetSettingValueAsync(channel.Id, "SMTP_PASSWORD");
        Assert.True(secureValue.IsSensitive);
        Assert.Null(secureValue.Value);
    }

    [Fact]
    public async Task SaveSetting_RejectsInvalidTypedValue()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new NotificationDbContext(options);
        var channelType = new NotificationChannelType { Code = "SMS", Name = "SMS" };
        dbContext.ChannelTypes.Add(channelType);
        await dbContext.SaveChangesAsync();
        dbContext.SettingDefinitions.Add(new NotificationSettingDefinition
        {
            ChannelTypeId = channelType.Id,
            Code = "TIMEOUT",
            Name = "Timeout",
            DataType = NotificationSettingDataType.Int,
            Enabled = true
        });
        var channel = new NotificationChannel { Name = "SMS", ChannelTypeId = channelType.Id, Type = ChannelType.Sms };
        dbContext.Channels.Add(channel);
        await dbContext.SaveChangesAsync();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["NotificationSecurity:EncryptionKey"] = "0123456789ABCDEF0123456789ABCDEF"
        }).Build();
        var service = new NotificationChannelConfigurationService(dbContext, new AesSecretProtector(configuration));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveSettingAsync(channel.Id, "TIMEOUT", "not-an-int"));
    }
}
