using Contracts.Domain;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Domain.Landowners;

public class Landowner : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public ContactInfo? Contact { get; private set; }
    public BankInfo? Bank { get; private set; }
    public IdentityInfo? Identity { get; private set; }
    public bool IsActive { get; private set; } = true;

    protected Landowner() { }

    private Landowner(
        string name,
        ContactInfo? contact,
        BankInfo? bank,
        IdentityInfo? identity,
        bool isActive)
    {
        Name = name;
        Contact = contact;
        Bank = bank;
        Identity = identity;
        IsActive = isActive;
    }

    public static Landowner Create(
        string name,
        ContactInfo? contact = null,
        BankInfo? bank = null,
        IdentityInfo? identity = null,
        bool isActive = true)
    {
        return new Landowner(name, contact, bank, identity, isActive);
    }

    public void Update(
        string name,
        ContactInfo? contact = null,
        BankInfo? bank = null,
        IdentityInfo? identity = null,
        bool? isActive = null)
    {
        Name = name;
        if (contact != null)
            Contact = contact;
        if (bank != null)
            Bank = bank;
        if (identity != null)
            Identity = identity;
        if (isActive.HasValue)
            IsActive = isActive.Value;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
