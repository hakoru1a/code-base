using Contracts.Domain;
using Contracts.Domain.Enums;
using Shared.Domain.ValueObjects;
using Shared.Domain.Enums;
using Shared.DTOs.BankAccount;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.Common;

namespace TLBIOMASS.Domain.Receivers;

public class Receiver : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public ContactInfo? Contact { get; private set; }
    public IdentityInfo? Identity { get; private set; }
    public bool IsDefault { get; private set; }
    
    private readonly List<BankAccount> _bankAccounts = new();
    public virtual IReadOnlyCollection<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    protected Receiver() { }

    private Receiver(string name, ContactInfo? contact, IdentityInfo? identity, bool isDefault, EntityStatus status)
    {
        Name = name;
        Contact = contact;
        Identity = identity;
        IsDefault = isDefault;
        Status = status;
    }

    public static Receiver Create(string name, ContactInfo? contact = null, IdentityInfo? identity = null, bool isDefault = false, EntityStatus status = EntityStatus.Active)
        => new Receiver(name, contact, identity, isDefault, status);

    public void Update(string name, ContactInfo? contact = null, IdentityInfo? identity = null, bool? isDefault = null, EntityStatus? status = null)
    {
        Name = name;
        if (contact != null) Contact = contact;
        if (identity != null) Identity = identity;
        if (isDefault.HasValue) IsDefault = isDefault.Value;
        if (status.HasValue) Status = status.Value;
    }

    public void SyncBankAccounts(IEnumerable<BankAccountSyncDto> dtos) => BankAccountManager.Sync(_bankAccounts, dtos, OwnerType.Receiver, Id);
    public void AddBankAccount(string? name, string? acc, bool setDef = true) => BankAccountManager.Add(_bankAccounts, name, acc, setDef, OwnerType.Receiver, Id);

    public void Activate() => Status = EntityStatus.Active;
    public void Deactivate() => Status = EntityStatus.Delete;
    public void SetDefault(bool isDefault) => IsDefault = isDefault;
}
