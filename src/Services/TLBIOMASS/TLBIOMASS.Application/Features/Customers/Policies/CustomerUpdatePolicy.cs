using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace TLBIOMASS.Application.Features.Customers.Policies;

[Policy("CUSTOMER:UPDATE", Description = "Cập nhật khách hàng")]
public class CustomerUpdatePolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        if (HasAnyRole(user, Roles.Admin, Roles.Manager))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow("Có quyền cập nhật khách hàng"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny("Bạn không có quyền cập nhật khách hàng"));
    }
}
