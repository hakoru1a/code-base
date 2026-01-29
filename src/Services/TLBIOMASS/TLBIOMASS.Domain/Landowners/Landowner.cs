using Contracts.Domain;
using Shared.Domain.ValueObjects;
using TLBIOMASS.Domain.BankAccounts;

namespace TLBIOMASS.Domain.Landowners;

public class Landowner : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public ContactInfo? Contact { get; private set; }
    public IdentityInfo? Identity { get; private set; }
    public bool IsActive { get; private set; } = true;

    public virtual ICollection<BankAccount> BankAccounts { get; private set; } = new List<BankAccount>();

    protected Landowner() { }

    private Landowner(
        string name,
        ContactInfo? contact,
        IdentityInfo? identity,
        bool isActive)
    {
        Name = name;
        Contact = contact;
        Identity = identity;
        IsActive = isActive;
    }

    public static Landowner Create(
        string name,
        ContactInfo? contact = null,
        IdentityInfo? identity = null,
        bool isActive = true)
    {
        return new Landowner(name, contact, identity, isActive);
    }

    public void Update(
        string name,
        ContactInfo? contact = null,
        IdentityInfo? identity = null,
        bool? isActive = null)
    {
        Name = name;
        if (contact != null)
            Contact = contact;
        if (identity != null)
            Identity = identity;
        if (isActive.HasValue)
            IsActive = isActive.Value;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
