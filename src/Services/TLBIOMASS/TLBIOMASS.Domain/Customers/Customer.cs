using Contracts.Domain;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Customers.Rules;
using Shared.Domain.ValueObjects;
using Contracts.Domain.Enums;

namespace TLBIOMASS.Domain.Customers;

public class Customer : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public ContactInfo? Contact { get; private set; }
    public string? TaxCode { get; private set; }
    // Status (EntityStatus) is inherited from EntityAuditBase -> EntityBase.

    protected Customer() { }

    private Customer(string name, ContactInfo? contact, string? taxCode, EntityStatus status)
    {
        Name = name;
        Contact = contact;
        TaxCode = taxCode;
        Status = status;
    }

    public static Customer Create(string name, ContactInfo? contact = null, string? taxCode = null)
    {
        return new Customer(name, contact, taxCode, EntityStatus.Active);
    }

    public void CheckTaxCodeUnique(ICustomerRepository repository)
    {
        CheckRule(new CustomerTaxCodeUniqueRule(repository, TaxCode, Id == 0 ? null : Id));
    }

    public void Update(string name, ContactInfo? contact = null, string? taxCode = null, EntityStatus? status = null)
    {
        Name = name;
        if (contact != null)
            Contact = contact;
        if (taxCode != null)
            TaxCode = taxCode;
        if (status.HasValue)
            Status = status.Value;
    }

    public void Activate() => Status = EntityStatus.Active;
    public void Deactivate() => Status = EntityStatus.Delete;
}
