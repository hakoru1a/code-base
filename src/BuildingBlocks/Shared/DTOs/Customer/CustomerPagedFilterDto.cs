namespace Shared.DTOs.Customer;

public class CustomerPagedFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
