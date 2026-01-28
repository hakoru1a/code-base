using Contracts.Domain;

namespace TLBIOMASS.Domain.Companies;

public class Company : EntityAuditBase<int>
{
    public string CompanyName { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public string? TaxCode { get; private set; }
    public string? Representative { get; private set; }
    public string? Position { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? IdentityCardNo { get; private set; }
    public string? IssuePlace { get; private set; }
    public DateTime? IssueDate { get; private set; }

    protected Company() { }

    private Company(
        string companyName,
        string? address,
        string? taxCode,
        string? representative,
        string? position,
        string? phoneNumber,
        string? email,
        string? identityCardNo,
        string? issuePlace,
        DateTime? issueDate)
    {
        CompanyName = companyName;
        Address = address;
        TaxCode = taxCode;
        Representative = representative;
        Position = position;
        PhoneNumber = phoneNumber;
        Email = email;
        IdentityCardNo = identityCardNo;
        IssuePlace = issuePlace;
        IssueDate = issueDate;
    }

    public static Company Create(
        string companyName,
        string? address = null,
        string? taxCode = null,
        string? representative = null,
        string? position = null,
        string? phoneNumber = null,
        string? email = null,
        string? identityCardNo = null,
        string? issuePlace = null,
        DateTime? issueDate = null)
    {
        return new Company(
            companyName,
            address,
            taxCode,
            representative,
            position,
            phoneNumber,
            email,
            identityCardNo,
            issuePlace,
            issueDate);
    }

    public void Update(
        string companyName,
        string? address,
        string? taxCode,
        string? representative,
        string? position,
        string? phoneNumber,
        string? email,
        string? identityCardNo,
        string? issuePlace,
        DateTime? issueDate)
    {
        CompanyName = companyName;
        Address = address;
        TaxCode = taxCode;
        Representative = representative;
        Position = position;
        PhoneNumber = phoneNumber;
        Email = email;
        IdentityCardNo = identityCardNo;
        IssuePlace = issuePlace;
        IssueDate = issueDate;
    }
}
