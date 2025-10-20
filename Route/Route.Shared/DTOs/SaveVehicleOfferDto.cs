using Route.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    // ---------------------------------------------------------
    // (Opcional) Creación/actualización explícita con validación
    // Recuerda: ya heredas CRUD de GenericController<T>.
    // Solo usa estos si quieres reglas extra.
    // -----------------------------------------------------------
    // DTO interno para Create/Update (incluye nuevos campos)
    // -----------------------------------------------------------
    public class SaveVehicleOfferDto
    {
        public int CapacityRequestId { get; set; }
        public int ProviderId { get; set; }
        public int? VehicleId { get; set; }
        public int Quantity { get; set; } = 1;
        public double OfferedWeightKg { get; set; }
        public double OfferedVolumeM3 { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "PEN";
        public PriceMode PriceMode { get; set; } = PriceMode.PerVehicle;
        public DateTime? ValidUntil { get; set; }
        public string? Notes { get; set; }
    }
}