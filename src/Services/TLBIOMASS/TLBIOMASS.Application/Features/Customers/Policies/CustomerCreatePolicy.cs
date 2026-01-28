using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace TLBIOMASS.Application.Features.Customers.Policies;

[Policy("CUSTOMER:CREATE", Description = "Tạo khách hàng mới")]
public class CustomerCreatePolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // Kiểm tra quyền hạn (Tạm thời cho phép Admin/Manager)
        if (HasAnyRole(user, Roles.Admin, Roles.Manager))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow("Có quyền tạo khách hàng"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny("Bạn không có quyền tạo khách hàng"));
    }
}
