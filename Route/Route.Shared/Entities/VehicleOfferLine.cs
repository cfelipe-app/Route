using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Entities
{
    public class VehicleOfferLine
    {
        public int Id { get; set; }

        public int OfferId { get; set; }
        public VehicleOffer Offer { get; set; } = null!;

        public int Seq { get; set; }                        // 1 = servicio principal
        public DateTime ServiceDate { get; set; }
        public TimeSpan WindowStart { get; set; }
        public TimeSpan WindowEnd { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }                  // regla: si Seq > 1, <= línea 1

        [MaxLength(300)]
        public string? Notes { get; set; }
    }
}