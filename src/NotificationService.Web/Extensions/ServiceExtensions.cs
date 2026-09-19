using Application.Channels;
using Application.Configuration;
using Application.Evaluators;
using Application.Services;
using Domain.Contracts.Repository;
using Domain.Contracts.Services;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NotificationService.Infrastructure.Security;
using NotificationService.Infrastructure.Templates;
using NotificationService.MapperProfiles;
using Persistence;
using Repository.Repositories;
using Service.DataConnectors;
using Service.Jobs;
using Service.PostActions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotificationService.Extensions
{
	/// <summary>
	///     Project Startup Helper Extensions
	/// </summary>
	public static class ServiceExtensions
	{

		#region Register Custom Services

		/// <summary> 
		///		Registers application services in the dependency injection container. 
		/// </summary>
		/// <param name="services"></param>
		public static void AddServices(this IServiceCollection services)
		{

			//services.ConfigureDbContext(configuration, "AVM SYSTEM API", logger);

			#region Repositories

			services.AddScoped<IConditionalRuleRepository, ConditionalRuleRepository>();
			services.AddScoped<IDataSourceConfigRepository, DataSourceConfigRepository>();
			services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();
			services.AddScoped<INotificationChannelRepository, NotificationChannelRepository>();
			services.AddScoped<INotificationChannelSettingRepository, NotificationChannelSettingRepository>();
			services.AddScoped<INotificationChannelSettingValueRepository, NotificationChannelSettingValueRepository>();
			services.AddScoped<INotificationChannelTypeRepository, NotificationChannelTypeRepository>();
			services.AddScoped<INotificationJobRepository, NotificationJobRepository>();
			services.AddScoped<INotificationSettingDefinitionRepository, NotificationSettingDefinitionRepository>();
			services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
			services.AddScoped<IPostExecutionActionRepository, PostExecutionActionRepository>();

			#endregion Repositories

			#region Services

			services.AddSingleton<ISecretProtector, AesSecretProtector>();

			services.AddScoped<IConditionalRuleService, ConditionalRuleService>();
			services.AddScoped<IDataSourceConfigService, DataSourceConfigService>();
			services.AddScoped<IExecutionLogService, ExecutionLogService>();
			services.AddScoped<INotificationChannelService, NotificationChannelService>();
			services.AddScoped<INotificationChannelSettingService, NotificationChannelSettingService>();
			services.AddScoped<INotificationChannelSettingValueService, NotificationChannelSettingValueService>();
			services.AddScoped<INotificationChannelTypeService, NotificationChannelTypeService>();
			services.AddScoped<INotificationJobService, NotificationJobService>();
			services.AddScoped<INotificationSettingDefinitionService, NotificationSettingDefinitionService>();
			services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
			services.AddScoped<IPostExecutionActionService, PostExecutionActionService>();

			services.AddScoped<INotificationChannelConfigurationService, NotificationChannelConfigurationService>();

			services.AddSingleton<ITemplateRenderer, FluidTemplateRenderer>();
			services.AddScoped<IDataSourceService, DataSourceService>();
			services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
			services.AddScoped<IPostExecutionProcessor, PostExecutionProcessor>();
			services.AddScoped<INotificationJobProcessor, NotificationJobProcessor>();
			services.AddScoped<ISchedulerService, QuartzSchedulerService>();

			services.AddScoped<INotificationChannelProvider, EmailChannelProvider>();
			services.AddScoped<INotificationChannelProvider, WebhookChannelProvider>();

			#endregion Services
		}

		#endregion Register Custom Services

		#region Private Declarations

		private static NullReferenceException GenerateException(string variableName, ILogger logger)
		{
			var message = $"Required variable [{variableName}] NOT FOUND. [{Assembly.GetExecutingAssembly().GetName().Name}] will fail to initialize.";
			logger.LogError("{m}", message);
			return new NullReferenceException(message);
		}

		private static NullReferenceException GetNullReferenceException(string variableName)
		{
			return new NullReferenceException($"Variable de ambiente $'{variableName}' no encontrada, esta" +
											  $" variable es requerida para poder inicializar el proceso de autenticación de  {Assembly.GetExecutingAssembly().GetName()?.Name}");
		}

		#endregion

		#region Services Configs

		/// <summary>
		///     Setup authentication and/or authorization.
		/// </summary>
		/// <param name="services"></param>
		/// <exception cref="Exception"></exception>
		//public static void ConfigureIdpAuthentication(this IServiceCollection services)
		//{
		//	var authority = Environment.GetEnvironmentVariable("IDP_AUTHORITY_URL");
		//	var ApiName = Environment.GetEnvironmentVariable("API_NAME");

		//	if (authority == null)
		//		throw GetNullReferenceException("IDP_AUTHORITY_URL");
		//	if (ApiName == null)
		//		throw GetNullReferenceException("API_NAME");

		//	services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
		//		.AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme, options =>
		//		{
		//			options.RequireHttpsMetadata = true;
		//			options.Authority = authority;
		//			options.ApiName = ApiName;
		//		});

		//	// Map roles to claims
		//	services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();
		//}

		/// <summary>
		///     Add AutoMapper configuration and profiles.
		/// </summary>
		/// <param name="services"></param>
		public static void ConfigureAutoMapper(this IServiceCollection services)
		{
			services.AddAutoMapper(c => c.AddProfile<MapperProfile>(),
				AppDomain.CurrentDomain.GetAssemblies());
		}

		/// <summary>
		///     API controllers configuration.
		/// </summary>
		/// <param name="services"></param>
		public static void ConfigureControllers(this IServiceCollection services)
		{
			services.AddControllers(o =>
			{
				o.SuppressAsyncSuffixInActionNames = true;
				o.RespectBrowserAcceptHeader = true;
				o.ReturnHttpNotAcceptable = true;
			})
				.AddJsonOptions(o =>
				{
					o.JsonSerializerOptions.WriteIndented = true;
					o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
					o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				})
				.AddXmlDataContractSerializerFormatters();
		}

		/// <summary>
		///     Setup CORS policies.
		/// </summary>
		/// <param name="services"></param>
		public static void ConfigureCors(this IServiceCollection services)
		{
			services.AddCors(o =>
			{
				o.AddPolicy("Base Cors Policy", builder => builder
					.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader());
			});
		}

		/// <summary>
		///     Setup encryption and algorithms for data protection.
		/// </summary>
		/// <param name="services"></param>
		public static void ConfigureDataProtection(this IServiceCollection services)
		{
			services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"/app/tmpkeys"))
				.UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
				{
					EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
					ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
				});
		}

		/// <summary>
		///     DB contexts setup.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			var loggerFactory = LoggerFactory
				.Create(builder =>
				{
					builder.ClearProviders();
					builder.AddConsole();
				});
			var logger = loggerFactory.CreateLogger<Program>();

			//static IInterceptor[] interceptorsFactory(IServiceProvider sp) => [sp.GetRequiredService<AuditInterceptor>()];
			services.AddDbContext<NotificationContext>(options =>
			{
				var connectionString = configuration.GetConnectionString("NotificationService")
					?? "Data Source=notification_service.db";
				options.UseSqlite(connectionString);
			});

			//services.ConfigureDbContext(configuration, "AVM SYSTEM API", logger, interceptorsFactory);
		}

		/// <summary>
		///     Configure forward headers.
		/// </summary>
		/// <param name="services"></param>
		public static void ConfigureForwardHeaders(this IServiceCollection services)
		{
			services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders =
					ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.All;
			});
		}

		/// <summary>
		///     Configure IIS integration.
		/// </summary>
		/// <param name="services"></param>
		public static void ConfigureIISIntegration(this IServiceCollection services)
		{
			services.Configure<IISOptions>(o => { });
		}

		/// <summary>
		///     Setup Swagger configurations (Services).
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		public static void ConfigureSwaggerGen(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			var keycloakBaseUrl = configuration["Keycloak:BaseUrl"];
			var keycloakRealm = configuration["Keycloak:Realm"];
			var keycloakClientId = configuration["Keycloak:ClientId"];

			var keycloakScopes = configuration["Keycloak:Scopes"]
				?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				?? [];

			var keycloakUrl =
				$"{keycloakBaseUrl}/realms/{keycloakRealm}";

			services.AddSwaggerGen(config =>
			{
				config.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Avm System Api",
					Version = "V1.0.0",
					Description =
						"This project holds all tickets persistence and handling logic for **Outbound Application**."
				});

				config.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.OAuth2,
					Description = "Keycloak OAuth2",
					Flows = new OpenApiOAuthFlows
					{
						AuthorizationCode = new OpenApiOAuthFlow
						{
							AuthorizationUrl = new Uri(
								$"{keycloakUrl}/protocol/openid-connect/auth"),

							TokenUrl = new Uri(
								$"{keycloakUrl}/protocol/openid-connect/token"),

							Scopes = keycloakScopes.ToDictionary(
								scope => scope,
								scope => scope)
						}
					}
				});

				config.AddSecurityRequirement(document =>
					new OpenApiSecurityRequirement
					{
						[
							new OpenApiSecuritySchemeReference(
								"Keycloak",
								document)
						] = [.. keycloakScopes]
					});

				config.CustomSchemaIds(type => type.ToString());
			});
		}
		#endregion Services Configs

		#region App Configs

		/// <summary>
		///     Setup Swagger configurations.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		/// <param name="configuration"></param>
		public static void ConfigureSwagger(
			this IApplicationBuilder app,
			IWebHostEnvironment env,
			IConfiguration configuration)
		{
			var useSwagger =
				Convert.ToBoolean(configuration["UseSwagger"]);

			if (env.IsProduction())
				app.UseHsts();

			if (!useSwagger && !env.IsDevelopment())
				return;

			var keycloakClientId =
				configuration["Keycloak:ClientId"];

			var keycloakScopes = configuration["Keycloak:Scopes"]
				?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				?? [];

			app.UseSwagger();

			app.UseSwaggerUI(options =>
			{
				options.OAuthClientId(keycloakClientId);
				options.OAuthUsePkce();
				options.OAuthScopes(keycloakScopes);
			});
		}

		/// <summary>
		///     Startup log.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="env"></param>
		/// <param name="logger"></param>
		/// <typeparam name="T"></typeparam>
		public static void StartupInfo<T>(this IApplicationBuilder _, IWebHostEnvironment env, ILogger<T> logger)
		{
			logger.LogInformation("Starting {N} @ {M}", env.ApplicationName, env.EnvironmentName);
		}

		#endregion App Configs

	}

}
