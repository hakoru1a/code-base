using Contracts.Domain;
using Shared.Domain.ValueObjects;
using Shared.Domain.Enums;
using Contracts.Domain.Enums;
using Shared.DTOs.BankAccount;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.Common;

namespace TLBIOMASS.Domain.Agencies;

public class Agency : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public ContactInfo? Contact { get; private set; }
    public IdentityInfo? Identity { get; private set; }
    
    // Bank Accounts logic (Composition)
    private readonly List<BankAccount> _bankAccounts = new();
    public virtual IReadOnlyCollection<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    protected Agency() { }

    private Agency(string name, ContactInfo? contact, IdentityInfo? identity, EntityStatus status)
    {
        Name = name;
        Contact = contact;
        Identity = identity;
        Status = status;
    }

    public static Agency Create(string name, ContactInfo? contact = null, IdentityInfo? identity = null, EntityStatus status = EntityStatus.Active)
        => new Agency(name, contact, identity, status);

    public void Update(string name, ContactInfo? contact = null, IdentityInfo? identity = null, EntityStatus? status = null)
    {
        Name = name;
        if (contact != null) Contact = contact;
        if (identity != null) Identity = identity;
        if (status.HasValue) Status = status.Value;
    }

    public void SyncBankAccounts(IEnumerable<BankAccountSyncDto> dtos) => BankAccountManager.Sync(_bankAccounts, dtos, OwnerType.Agency, Id);
    public void AddBankAccount(string? name, string? acc, bool setDef = true) => BankAccountManager.Add(_bankAccounts, name, acc, setDef, OwnerType.Agency, Id);

    public void Activate() => Status = EntityStatus.Active;
    public void Deactivate() => Status = EntityStatus.Delete;
}
