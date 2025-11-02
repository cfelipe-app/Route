using Route.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    //public class VehicleOfferDto
    //{
    //    public int Id { get; set; }
    //    public int CapacityRequestId { get; set; }

    //    public int ProviderId { get; set; }
    //    public string ProviderName { get; set; } = string.Empty;

    //    public int Quantity { get; set; }
    //    public double OfferedWeightKg { get; set; }
    //    public double OfferedVolumeM3 { get; set; }

    //    public decimal Price { get; set; }
    //    public string Currency { get; set; } = "PEN";

    //    public PriceMode PriceMode { get; set; }
    //    public string PriceModeText { get; set; } = string.Empty;

    //    public VehicleOfferStatus Status { get; set; }
    //    public string StatusText { get; set; } = string.Empty;

    //    public DateTime CreatedAt { get; set; }
    //    public DateTime? ValidUntil { get; set; }
    //    public string? Notes { get; set; }
    //}

    public class VehicleOfferDto
    {
        public int Id { get; set; }
        public int CapacityRequestId { get; set; }
        public DateTime ReqServiceDate { get; set; }        // NUEVO
        public string? ReqZone { get; set; }               // NUEVO
        public TimeSpan? ReqWindowStart { get; set; }       // NUEVO
        public TimeSpan? ReqWindowEnd { get; set; }         // NUEVO

        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public double OfferedWeightKg { get; set; }
        public double OfferedVolumeM3 { get; set; }

        public decimal Price { get; set; }
        public string Currency { get; set; } = "PEN";

        public PriceMode PriceMode { get; set; }
        public string PriceModeText { get; set; } = string.Empty;

        public VehicleOfferStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? Notes { get; set; }

        // === NUEVO: metadata del vehículo (si la oferta es por placa) ===
        public int? VehicleId { get; set; }

        public string? VehiclePlate { get; set; }
        public double? VehicleCapacityKg { get; set; }
        public double? VehicleCapacityVolM3 { get; set; }
        public int? VehicleSeats { get; set; }
        public string? VehicleTonnageLabel { get; set; }
    }
}