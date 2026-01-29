using Contracts.Domain.Enums;

namespace Shared.DTOs.Receiver;

public class ReceiverCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? IssuedPlace { get; set; }
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    public string? Note { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
