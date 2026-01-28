using Contracts.Domain.Interface;
using System.Linq.Expressions;

namespace TLBIOMASS.Domain.Payments.Specifications;

public class PaymentDetailSearchSpecification : ISpecification<PaymentDetail>
{
    private readonly string _searchTerm;

    public PaymentDetailSearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(PaymentDetail entity)
    {
        return ToExpression().Compile()(entity);
    }

    public Expression<Func<PaymentDetail, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;
            
        return x => x.PaymentCode.ToLower().Contains(_searchTerm) || 
                    (x.Note != null && x.Note.ToLower().Contains(_searchTerm));
    }
}
