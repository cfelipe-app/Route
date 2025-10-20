using Route.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    public class VehicleOfferCreateDto
    {
        [Required]
        public int CapacityRequestId { get; set; }

        [Required]
        public int ProviderId { get; set; }

        // Si ofreces una placa específica, informa VehicleId (1 unidad).
        public int? VehicleId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity debe ser >= 1")]
        public int Quantity { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "OfferedWeightKg no puede ser negativo")]
        public double OfferedWeightKg { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "OfferedVolumeM3 no puede ser negativo")]
        public double OfferedVolumeM3 { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price no puede ser negativo")]
        public decimal Price { get; set; }

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "PEN";

        [Required]
        public PriceMode PriceMode { get; set; } = PriceMode.PerVehicle;

        public DateTime? ValidUntil { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Ctor vacío (model binding)
        public VehicleOfferCreateDto()
        { }

        // Ctor de conveniencia (opcional)
        public VehicleOfferCreateDto(
            int capacityRequestId, int providerId,
            int? vehicleId, int quantity,
            double offeredWeightKg, double offeredVolumeM3,
            decimal price, string currency,
            PriceMode priceMode, DateTime? validUntil, string? notes)
        {
            CapacityRequestId = capacityRequestId;
            ProviderId = providerId;
            VehicleId = vehicleId;
            Quantity = quantity;
            OfferedWeightKg = offeredWeightKg;
            OfferedVolumeM3 = offeredVolumeM3;
            Price = price;
            Currency = currency;
            PriceMode = priceMode;
            ValidUntil = validUntil;
            Notes = notes;
        }
    }
}