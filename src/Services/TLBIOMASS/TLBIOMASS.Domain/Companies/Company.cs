using Contracts.Domain;
using Shared.Domain.ValueObjects;
using TLBIOMASS.Domain.Companies.ValueObjects;

namespace TLBIOMASS.Domain.Companies;

public class Company : EntityAuditBase<int>
{
    public string CompanyName { get; private set; } = string.Empty;
    public string? TaxCode { get; private set; }
    public RepresentativeInfo? Representative { get; private set; }
    public ContactInfo? Contact { get; private set; }
    public IdentityInfo? Identity { get; private set; }

    protected Company() { }

    private Company(
        string companyName,
        string? taxCode,
        RepresentativeInfo? representative,
        ContactInfo? contact,
        IdentityInfo? identity)
    {
        CompanyName = companyName;
        TaxCode = taxCode;
        Representative = representative;
        Contact = contact;
        Identity = identity;
    }

    public static Company Create(
        string companyName,
        string? taxCode = null,
        RepresentativeInfo? representative = null,
        ContactInfo? contact = null,
        IdentityInfo? identity = null)
    {
        return new Company(
            companyName,
            taxCode,
            representative,
            contact,
            identity);
    }

    public void Update(
        string companyName,
        string? taxCode,
        RepresentativeInfo? representative,
        ContactInfo? contact,
        IdentityInfo? identity)
    {
        CompanyName = companyName;
        TaxCode = taxCode;
        Representative = representative;
        Contact = contact;
        Identity = identity;
    }
}
