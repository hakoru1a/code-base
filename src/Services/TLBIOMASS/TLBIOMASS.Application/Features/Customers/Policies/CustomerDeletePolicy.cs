using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace TLBIOMASS.Application.Features.Customers.Policies;

[Policy("CUSTOMER:DELETE", Description = "Xóa khách hàng")]
public class CustomerDeletePolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        if (HasAnyRole(user, Roles.Admin))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow("Có quyền xóa khách hàng"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny("Chỉ Admin mới có quyền xóa khách hàng"));
    }
}
