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

        // Placa específica (opcional)
        public int? VehicleId { get; set; }

        public Vehicle? Vehicle { get; set; }

        // ===== Campos “legado” del encabezado (compatibilidad) =====
        // Puedes seguir mostrándolos en la grilla actual; al crear/editar
        // el backend los usará para generar la línea 1 si no envías Lines.

        public int Quantity { get; set; } = 1;
        public double OfferedWeightKg { get; set; }
        public double OfferedVolumeM3 { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }                 // precio “línea 1”

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "PEN";

        public PriceMode PriceMode { get; set; } = PriceMode.PerVehicle;

        public VehicleOfferStatus Status { get; set; } = VehicleOfferStatus.Draft;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ValidUntil { get; set; }

        public DateTime? DecisionAt { get; set; }

        [MaxLength(80)]
        public string? DecidedBy { get; set; }

        // ===== NUEVO: líneas de servicio =====
        public ICollection<VehicleOfferLine> Lines { get; set; } = new List<VehicleOfferLine>();
    }
}