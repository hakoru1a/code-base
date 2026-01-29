using MediatR;
using Shared.DTOs.BankAccount;

namespace TLBIOMASS.Application.Features.Agencies.Commands.UpdateAgency
{
    public class UpdateAgencyCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? IdentityCard { get; set; }
        public string? IssuePlace { get; set; }
        public DateTime? IssueDate { get; set; }
        public bool IsActive { get; set; }
        public List<BankAccountSyncDto> BankAccounts { get; set; } = new();
    }
}
