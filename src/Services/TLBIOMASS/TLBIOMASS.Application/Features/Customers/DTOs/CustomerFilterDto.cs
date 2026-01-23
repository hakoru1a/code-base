using TLBIOMASS.Application.Common.DTOs;

namespace TLBIOMASS.Application.Features.Customers.DTOs;


public class CustomerFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
