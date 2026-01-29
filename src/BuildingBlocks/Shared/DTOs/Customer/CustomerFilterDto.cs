using Shared.SeedWork;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Customer;

public class CustomerFilterDto : RequestParameter
{
    public string? Search { get; set; }
    public EntityStatus? Status { get; set; }
}
