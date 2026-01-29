using Contracts.Domain;
using Shared.Domain.ValueObjects;
using Shared.Domain.Enums;
using Shared.DTOs.BankAccount;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.Common;

namespace TLBIOMASS.Domain.Receivers;

public class Receiver : BankAccountOwner
{
    public string Name { get; private set; } = string.Empty;
    public ContactInfo? Contact { get; private set; }
    public IdentityInfo? Identity { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;

    protected override OwnerType OwnerType => OwnerType.Receiver;

    protected Receiver() { }

    private Receiver(
        string name,
        ContactInfo? contact,
        IdentityInfo? identity,
        bool isDefault,
        bool isActive)
    {
        Name = name;
        Contact = contact;
        Identity = identity;
        IsDefault = isDefault;
        IsActive = isActive;
    }

    public static Receiver Create(
        string name,
        ContactInfo? contact = null,
        IdentityInfo? identity = null,
        bool isDefault = false,
        bool isActive = true)
    {
        return new Receiver(name, contact, identity, isDefault, isActive);
    }

    public void Update(
        string name,
        ContactInfo? contact = null,
        IdentityInfo? identity = null,
        bool? isDefault = null,
        bool? isActive = null)
    {
        Name = name;
        if (contact != null)
            Contact = contact;
        if (identity != null)
            Identity = identity;
        if (isDefault.HasValue)
            IsDefault = isDefault.Value;
        if (isActive.HasValue)
            IsActive = isActive.Value;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetDefault(bool isDefault) => IsDefault = isDefault;
}
