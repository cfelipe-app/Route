using Microsoft.AspNetCore.Identity;
using Route.Shared.Entities;

namespace Route.Backend.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public int? ProviderId { get; set; }
        public Provider? Provider { get; set; }

        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }
    }
}