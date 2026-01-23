using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Customers.Specifications;

public class CustomerSearchSpecification : ISpecification<Customer>
{
    private readonly string _searchTerm;

    public CustomerSearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(Customer customer)
    {
        return ToExpression().Compile()(customer);
    }

    public Expression<Func<Customer, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;

        return c => c.TenKhachHang.ToLower().Contains(_searchTerm) ||
                   (c.DienThoai != null && c.DienThoai.Contains(_searchTerm)) ||
                   (c.Email != null && c.Email.ToLower().Contains(_searchTerm)) ||
                   (c.MaSoThue != null && c.MaSoThue.Contains(_searchTerm));
    }
}
