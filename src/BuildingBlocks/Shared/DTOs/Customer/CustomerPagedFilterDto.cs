using Shared.DTOs;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Customer;

public class CustomerPagedFilterDto : BaseFilterDto
{
    public string? Search { get; set; }
    public EntityStatus? Status { get; set; }
}
