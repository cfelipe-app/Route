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
    // ---------------------------------------------------------

    public class SaveVehicleOfferDto
    {
        public int CapacityRequestId { get; set; }
        public int ProviderId { get; set; }
        public int VehicleId { get; set; }
        public double OfferedWeightKg { get; set; }
        public double OfferedVolumeM3 { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "PEN";
        public string? Notes { get; set; }
    }
}