using System.Security.Claims;

namespace Route.Backend.Security
{
    public static class ClaimsHelper
    {
        public static int? GetProviderId(ClaimsPrincipal user)
            => int.TryParse(user.FindFirstValue("provider_id"), out var id) ? id : null;

        public static int? GetDriverId(ClaimsPrincipal user)
            => int.TryParse(user.FindFirstValue("driver_id"), out var id) ? id : null;
    }
}