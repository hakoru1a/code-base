using Contracts.Domain;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Customers.Rules;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Domain.Customers;

public class Customer : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public ContactInfo? Contact { get; private set; }
    public string? TaxCode { get; private set; }
    public bool IsActive { get; private set; } = true;

    protected Customer() { }

    private Customer(string name, ContactInfo? contact, string? taxCode, bool isActive)
    {
        Name = name;
        Contact = contact;
        TaxCode = taxCode;
        IsActive = isActive;
    }

    public static Customer Create(string name, ContactInfo? contact = null, string? taxCode = null)
    {
        return new Customer(name, contact, taxCode, true);
    }

    public void CheckTaxCodeUnique(ICustomerRepository repository)
    {
        CheckRule(new CustomerTaxCodeUniqueRule(repository, TaxCode, Id == 0 ? null : Id));
    }

    public void Update(string name, ContactInfo? contact = null, string? taxCode = null)
    {
        Name = name;
        if (contact != null)
            Contact = contact;
        if (taxCode != null)
            TaxCode = taxCode;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
