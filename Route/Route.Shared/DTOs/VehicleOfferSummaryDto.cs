using Route.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    public class VehicleOfferSummaryDto
    {
        public int Id { get; set; }
        public int CapacityRequestId { get; set; }
        public int ProviderId { get; set; }
        public int? VehicleId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "PEN";

        public PriceMode PriceMode { get; set; } = PriceMode.PerVehicle;
        public VehicleOfferStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Ctor vacío
        public VehicleOfferSummaryDto()
        { }

        // Ctor de conveniencia (opcional)
        public VehicleOfferSummaryDto(
            int id, int capacityRequestId, int providerId,
            int? vehicleId, int quantity,
            decimal price, string currency, PriceMode priceMode,
            VehicleOfferStatus status, DateTime createdAt)
        {
            Id = id;
            CapacityRequestId = capacityRequestId;
            ProviderId = providerId;
            VehicleId = vehicleId;
            Quantity = quantity;
            Price = price;
            Currency = currency;
            PriceMode = priceMode;
            Status = status;
            CreatedAt = createdAt;
        }
    }
}