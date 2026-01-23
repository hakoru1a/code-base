using Microsoft.EntityFrameworkCore;
using Contracts.Domain.Interface;
using TLBIOMASS.Domain.Customers.Interfaces;

namespace TLBIOMASS.Domain.Customers.Rules;

public class CustomerMaSoThueUniqueRule : IBusinessRule
{
    private readonly ICustomerRepository _repository;
    private readonly string? _maSoThue;
    private readonly int? _excludeId;

    public CustomerMaSoThueUniqueRule(ICustomerRepository repository, string? maSoThue, int? excludeId = null)
    {
        _repository = repository;
        _maSoThue = maSoThue;
        _excludeId = excludeId;
    }

    public bool IsBroken()
    {
        if (string.IsNullOrEmpty(_maSoThue)) return false;
        
        return _repository.FindByCondition(c => c.MaSoThue == _maSoThue && (_excludeId == null || c.Id != _excludeId))
            .AnyAsync().GetAwaiter().GetResult();
    }

    public string Message => "Mã số thuế này đã tồn tại trong hệ thống";
    public string Code => "Customer.MaSoThueUnique";
}
