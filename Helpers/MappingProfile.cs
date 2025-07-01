using AutoMapper;
using HRSystem.API.DTOs;
using HRSystem.API.Models;

namespace HRSystem.API.Helpers
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            
            CreateMap<Employee, EmployeeDto>().ReverseMap();
            CreateMap<EmployeeAsset, EmployeeAssetDto>().ReverseMap();
            
            CreateMap<Asset, AssetDto>().ReverseMap();
            CreateMap<AssetDto, Asset>().ForMember(dest => dest.Id, opt => opt.Ignore());


            // Add more mappings as needed
            CreateMap<Shift, ShiftDto>().ReverseMap();
            CreateMap<EmployeeShift, EmployeeShiftDto>().ReverseMap();
            CreateMap<Attendance, AttendanceDto>().ReverseMap();
            CreateMap<Payroll, PayrollDto>().ReverseMap();
            CreateMap<PayrollCreateDto, Payroll>();

            CreateMap<LeaveRequest, LeaveRequestDto>().ReverseMap();
            CreateMap<LeaveRequest, LeaveRequestResponseDto>().ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FirstName + " " + src.Employee.LastName));
           
            CreateMap<FinalSettlement, FinalSettlementDto>().ReverseMap();
            CreateMap<GratuityReport, GratuityReportDto>().ReverseMap();
            CreateMap<IncrementHistory, IncrementHistoryDto>().ReverseMap();

            CreateMap<IncrementHistory, IncrementHistoryDto>()
    .ForMember(dest => dest.Employee, opt => opt.MapFrom(src => src.Employee));

            CreateMap<Employee, SimpleEmployeeDto>();



        }
    }

}
