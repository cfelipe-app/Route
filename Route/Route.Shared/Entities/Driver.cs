using Route.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Route.Shared.Entities
{
    public class Driver : IEntityWithId
    {
        public int Id { get; set; }

        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? DocumentId { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(120)]
        public string? Email { get; set; }

        [MaxLength(40)]
        public string? LicenseNumber { get; set; }

        [MaxLength(20)]
        public string? LicenseClass { get; set; }

        public bool IsActive { get; set; } = true;

        public int ProviderId { get; set; }
        public Provider? Provider { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RoutePlan> Routes { get; set; } = new List<RoutePlan>();
    }
}