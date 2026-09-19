using AutoMapper;

namespace NotificationService.Web.MapperProfiles
{
	/// <summary>
	///     Avm DB Model to Contracts mapper Profile
	/// </summary>
	public class AvmMapperProfile : Profile
	{
		/// <summary>
		///     Avm DB Model to Contracts mapper Profile
		/// </summary>
		public AvmMapperProfile()
		{
			SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
			DestinationMemberNamingConvention = new PascalCaseNamingConvention();

			CreateMap<string, DateTime>().ConvertUsing<DateTimeTypeConverter>();
			//CreateMap<ExternalApplications, ExternalApplicationsCreateRequest>().ReverseMap();

			//ConditionalRule
			//DataSourceConfig
			//ExecutionLog
			//NotificationChannel
			//NotificationChannelSetting
			//NotificationChannelSettingValue
			//NotificationChannelSettingValue
			//NotificationChannelType
			//NotificationJob
			//NotificationSettingDefinition
			//NotificationTemplate
			//PostExecutionAction

			//#region Consortium

			//CreateMap<Consortium, ConsortiumUpdateRequestLocal>();
			//CreateMap<ConsortiumUpdateRequestLocal, Consortium>()
			//	.ForMember(d => d.Location, o => o.MapFrom(s => s.GetLocation()));

			//CreateMap<Consortium, ConsortiumCreateRequestLocal>();
			//CreateMap<ConsortiumCreateRequestLocal, Consortium>()
			//	.ForMember(d => d.Location, o => o.MapFrom(s => s.GetLocation()));

			//CreateMap<Consortium, ConsortiumViewLocal>()
			//	.ForMember(d => d.Latitude,
			//		o => o.MapFrom(s => s.Location.Coordinate.Y))
			//	.ForMember(d => d.Longitude,
			//		o => o.MapFrom(s => s.Location.Coordinate.X));

			//CreateMap<Consortium, ConsortiumCreateRequest>().ReverseMap();
			//CreateMap<Consortium, ConsortiumUpdateRequest>().ReverseMap();
			//CreateMap<Consortium, ConsortiumView>().ReverseMap();

			//#endregion


		}
	}

	/// <summary>
	///     DateTime to String mapper Converter
	/// </summary>
	public class DateTimeTypeConverter : ITypeConverter<string, DateTime>
	{
		/// <summary>
		///     DateTime to String mapper Converter
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public DateTime Convert(string source, DateTime destination, ResolutionContext context)
		{
			return System.Convert.ToDateTime(source);
		}
	}

}
