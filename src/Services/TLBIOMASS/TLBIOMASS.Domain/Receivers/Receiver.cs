using Contracts.Domain;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Domain.Receivers;

public class Receiver : EntityBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public ContactInfo? Contact { get; private set; }
    public BankInfo? Bank { get; private set; }
    public IdentityInfo? Identity { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    protected Receiver() { }

    private Receiver(
        string name,
        ContactInfo? contact,
        BankInfo? bank,
        IdentityInfo? identity,
        bool isDefault,
        bool isActive)
    {
        Name = name;
        Contact = contact;
        Bank = bank;
        Identity = identity;
        IsDefault = isDefault;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public static Receiver Create(
        string name,
        ContactInfo? contact = null,
        BankInfo? bank = null,
        IdentityInfo? identity = null,
        bool isDefault = false,
        bool isActive = true)
    {
        return new Receiver(name, contact, bank, identity, isDefault, isActive);
    }

    public void Update(
        string name,
        ContactInfo? contact = null,
        BankInfo? bank = null,
        IdentityInfo? identity = null,
        bool? isDefault = null,
        bool? isActive = null)
    {
        Name = name;
        if (contact != null)
            Contact = contact;
        if (bank != null)
            Bank = bank;
        if (identity != null)
            Identity = identity;
        if (isDefault.HasValue)
            IsDefault = isDefault.Value;
        if (isActive.HasValue)
            IsActive = isActive.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetDefault(bool isDefault) => IsDefault = isDefault;
}
