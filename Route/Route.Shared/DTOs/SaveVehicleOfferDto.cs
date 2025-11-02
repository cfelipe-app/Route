using Route.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    public class SaveVehicleOfferDto
    {
        public int CapacityRequestId { get; set; }
        public int ProviderId { get; set; }
        public int? VehicleId { get; set; }

        public int Quantity { get; set; }
        public double OfferedWeightKg { get; set; }
        public double OfferedVolumeM3 { get; set; }
        public decimal Price { get; set; }

        public string Currency { get; set; } = "PEN";
        public PriceMode PriceMode { get; set; } = PriceMode.PerVehicle;

        public DateTime? ValidUntil { get; set; }
        public string? Notes { get; set; }

        /// <summary>
        /// Líneas opcionales: una por día/ventana con su precio.
        /// </summary>
        public List<SaveVehicleOfferLineDto>? Lines { get; set; }

        // ctor vacío para serialización
        public SaveVehicleOfferDto()
        { }

        public SaveVehicleOfferDto(
            int capacityRequestId, int providerId, int? vehicleId,
            int quantity, double offeredWeightKg, double offeredVolumeM3,
            decimal price, string currency, PriceMode priceMode,
            DateTime? validUntil, string? notes,
            List<SaveVehicleOfferLineDto>? lines = null)
        {
            CapacityRequestId = capacityRequestId;
            ProviderId = providerId;
            VehicleId = vehicleId;
            Quantity = quantity;
            OfferedWeightKg = offeredWeightKg;
            OfferedVolumeM3 = offeredVolumeM3;
            Price = price;
            Currency = string.IsNullOrWhiteSpace(currency) ? "PEN" : currency;
            PriceMode = priceMode;
            ValidUntil = validUntil;
            Notes = notes;
            Lines = lines;
        }
    }
}