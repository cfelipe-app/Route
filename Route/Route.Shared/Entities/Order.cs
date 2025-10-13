using Route.Shared.Enums;
using Route.Shared.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Route.Shared.Entities
{
    public class Order : IEntityWithId
    {
        public int Id { get; set; }

        [MaxLength(40)]
        public string? ExternalOrderNo { get; set; }

        [Required, MaxLength(150)]
        public string CustomerName { get; set; } = null!;

        [MaxLength(20)]
        public string? CustomerTaxId { get; set; }

        [Required, MaxLength(220)]
        public string Address { get; set; } = null!;

        [MaxLength(100)] public string? District { get; set; }
        [MaxLength(100)] public string? Province { get; set; }
        [MaxLength(100)] public string? Department { get; set; }

        public decimal WeightKg { get; set; }
        public decimal VolumeM3 { get; set; }
        public int Packages { get; set; }
        public decimal AmountTotal { get; set; }     // precisión en DbContext

        [MaxLength(30)]
        public string? PaymentMethod { get; set; }

        public decimal? Latitude { get; set; }        // precisión en DbContext
        public decimal? Longitude { get; set; }       // precisión en DbContext

        public DateTime? BillingDate { get; set; }
        public DateTime? ScheduledDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; }       // default GETUTCDATE() en DbContext

        [MaxLength(40)] public string? InvoiceDoc { get; set; }
        public DateTime? InvoiceDate { get; set; }

        [MaxLength(40)] public string? GuideDoc { get; set; }
        public DateTime? GuideDate { get; set; }

        [MaxLength(20)] public string? TransportRuc { get; set; }
        [MaxLength(120)] public string? TransportName { get; set; }
        [MaxLength(120)] public string? DeliveryDeptGuide { get; set; }

        // Navegación
        public ICollection<RouteOrder> RouteOrders { get; set; } = new List<RouteOrder>();
    }
}