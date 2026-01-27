using MediatR;

namespace TLBIOMASS.Application.Features.Receivers.Commands.UpdateReceiver;

public class UpdateReceiverCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? IssuedPlace { get; set; }
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
