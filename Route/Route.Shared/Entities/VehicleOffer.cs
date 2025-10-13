using Route.Shared.Enums;
using Route.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Entities
{
    public class VehicleOffer : IEntityWithId
    {
        public int Id { get; set; }

        public int CapacityRequestId { get; set; }
        public CapacityRequest CapacityRequest { get; set; } = null!;

        public int ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public double OfferedWeightKg { get; set; }
        public double OfferedVolumeM3 { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "PEN";

        public VehicleOfferStatus Status { get; set; } = VehicleOfferStatus.Draft;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Timestamp de la decisión
        public DateTime? DecisionAt { get; set; }

        // (Opcional, si también quieres guardar quién decide)
        [MaxLength(80)]
        public string? DecidedBy { get; set; }
    }
}