using AutoMapper;
using PrintingTools.Application.DTOs.PrintJobs;
using PrintingTools.Application.DTOs.Users;
using PrintingTools.Domain.Entities;

namespace PrintingTools.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmedAt.HasValue));
        
        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmedAt.HasValue))
            .ForMember(dest => dest.TotalPrintJobs, opt => opt.Ignore())
            .ForMember(dest => dest.ActivePrintJobs, opt => opt.Ignore());
        
        CreateMap<PrintJob, PrintJobDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.GetFullName() : ""))
            .ForMember(dest => dest.FileSizeFormatted, opt=> opt.MapFrom(src => FormatFileSize(src.FileSizeBytes)))
            .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}