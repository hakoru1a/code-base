using Shared.SeedWork;

namespace Shared.DTOs.Customer;

public class CustomerFilterDto : RequestParameter
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
