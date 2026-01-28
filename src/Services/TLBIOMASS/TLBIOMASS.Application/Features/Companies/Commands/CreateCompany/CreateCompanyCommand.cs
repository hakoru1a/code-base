using MediatR;

namespace TLBIOMASS.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommand : IRequest<long>
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public string? Representative { get; set; }
    public string? Position { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? IdentityCardNo { get; set; }
    public string? IssuePlace { get; set; }
    public DateTime? IssueDate { get; set; }
}
