using Contracts.Domain;
using Shared.Domain.Enums;
using Shared.DTOs.BankAccount;
using TLBIOMASS.Domain.BankAccounts;

namespace TLBIOMASS.Domain.Common;

public abstract class BankAccountOwner : EntityAuditBase<int>
{
    protected readonly List<BankAccount> _bankAccounts = new();
    public virtual IReadOnlyCollection<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    protected abstract OwnerType OwnerType { get; }

    public void ApplyBankAccountChange(BankAccountSyncDto dto)
    {
        switch (dto.Action)
        {
            case BankAccountAction.Create:
                AddBankAccount(dto.BankName, dto.AccountNumber, dto.IsDefault);
                break;
            case BankAccountAction.Update:
                UpdateBankAccount(dto);
                break;
            case BankAccountAction.Delete:
                RemoveBankAccount(dto);
                break;
        }

        EnsureSingleDefault();
    }

    public void AddBankAccount(string bankName, string accountNumber, bool isDefault = false)
    {
        // Logic mới: Nếu là tài khoản đầu tiên, tự động cho là default nếu chưa có cái nào default
        bool shouldBeDefault = isDefault || !_bankAccounts.Any(x => x.IsDefault);

        _bankAccounts.Add(BankAccount.Create(
            bankName,
            accountNumber,
            this.OwnerType,
            this.Id,
            shouldBeDefault));

        if (shouldBeDefault)
        {
            EnsureSingleDefault();
        }
    }

    private void UpdateBankAccount(BankAccountSyncDto dto)
    {
        var account = _bankAccounts.FirstOrDefault(x => x.Id == dto.Id);
        if (account != null)
        {
            account.Update(dto.BankName, dto.AccountNumber, dto.IsDefault);
        }
    }

    private void RemoveBankAccount(BankAccountSyncDto dto)
    {
        var account = _bankAccounts.FirstOrDefault(x => x.Id == dto.Id);
        if (account != null)
        {
            _bankAccounts.Remove(account);
        }
    }

    private void EnsureSingleDefault()
    {
        var defaults = _bankAccounts.Where(x => x.IsDefault).ToList();
        if (defaults.Count <= 1) return;

        // Giữ lại cái cuối cùng được set default, các cái trước đó set về false
        foreach (var acc in defaults.SkipLast(1))
        {
            acc.SetDefault(false);
        }
    }
}
