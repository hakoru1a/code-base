using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace TLBIOMASS.Application.Features.Customers.Policies;

[Policy("CUSTOMER:VIEW", Description = "Xem khách hàng")]
public class CustomerViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        return Task.FromResult(PolicyEvaluationResult.Allow("Tất cả người dùng đều có thể xem"));
    }
}
