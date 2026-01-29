using Shared.Domain.Enums;
using Shared.DTOs.BankAccount;
using TLBIOMASS.Domain.BankAccounts;

namespace TLBIOMASS.Domain.Common;

public static class BankAccountManager
{
    public static void Sync(List<BankAccount> existingAccounts, IEnumerable<BankAccountSyncDto> dtos, OwnerType ownerType, int ownerId)
    {
        if (dtos == null) return;

        foreach (var dto in dtos)
        {
            switch (dto.Action)
            {
                case BankAccountAction.Create:
                    AddInternal(existingAccounts, dto.BankName, dto.AccountNumber, dto.IsDefault, ownerType, ownerId);
                    break;
                case BankAccountAction.Update:
                    UpdateInternal(existingAccounts, dto);
                    break;
                case BankAccountAction.Delete:
                    RemoveInternal(existingAccounts, dto);
                    break;
            }
        }

        EnsureSingleDefault(existingAccounts);
    }

    public static void Add(List<BankAccount> accounts, string? bankName, string? accountNumber, bool isDefault, OwnerType ownerType, int ownerId)
    {
        AddInternal(accounts, bankName, accountNumber, isDefault, ownerType, ownerId);
        EnsureSingleDefault(accounts);
    }

    private static void AddInternal(List<BankAccount> accounts, string? bankName, string? accountNumber, bool isDefault, OwnerType ownerType, int ownerId)
    {
        if (string.IsNullOrWhiteSpace(accountNumber)) return;

        bool shouldBeDefault = isDefault || !accounts.Any(x => x.IsDefault);

        accounts.Add(BankAccount.Create(
            bankName ?? string.Empty,
            accountNumber,
            ownerType,
            ownerId,
            shouldBeDefault));
    }

    private static void UpdateInternal(List<BankAccount> accounts, BankAccountSyncDto dto)
    {
        var account = accounts.FirstOrDefault(x => x.Id == dto.Id);
        if (account != null)
        {
            account.Update(dto.BankName ?? string.Empty, dto.AccountNumber ?? string.Empty, dto.IsDefault);
        }
    }

    private static void RemoveInternal(List<BankAccount> accounts, BankAccountSyncDto dto)
    {
        var account = accounts.FirstOrDefault(x => x.Id == dto.Id);
        if (account != null)
        {
            accounts.Remove(account);
        }
    }

    private static void EnsureSingleDefault(List<BankAccount> accounts)
    {
        var defaults = accounts.Where(x => x.IsDefault).ToList();
        if (defaults.Count <= 1) return;

        foreach (var acc in defaults.SkipLast(1))
        {
            acc.SetDefault(false);
        }
    }
}
