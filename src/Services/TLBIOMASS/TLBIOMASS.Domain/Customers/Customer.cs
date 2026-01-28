using Contracts.Domain;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Customers.Rules;

namespace TLBIOMASS.Domain.Customers;

public class Customer : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? Note { get; private set; }
    public string? Email { get; private set; }
    public string? TaxCode { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Protected constructor for EF Core
    protected Customer() { }

    // Domain constructor
    public Customer(
        string name,
        string? phone = null,
        string? address = null,
        string? email = null,
        string? taxCode = null,
        string? note = null)
    {
        Name = name;
        Phone = phone;
        Address = address;
        Email = email;
        TaxCode = taxCode;
        Note = note;
        IsActive = true;
    }

    public static Customer Create(
        string name,
        string? phone = null,
        string? address = null,
        string? email = null,
        string? taxCode = null,
        string? note = null)
    {
        return new Customer(name, phone, address, email, taxCode, note);
    }

    public void CheckTaxCodeUnique(ICustomerRepository repository)
    {
        CheckRule(new CustomerTaxCodeUniqueRule(repository, TaxCode, Id == 0 ? null : Id));
    }

    public void Update(
        string name,
        string? phone,
        string? address,
        string? email,
        string? taxCode,
        string? note)
    {
        Name = name;
        Phone = phone;
        Address = address;
        Email = email;
        TaxCode = taxCode;
        Note = note;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

}
