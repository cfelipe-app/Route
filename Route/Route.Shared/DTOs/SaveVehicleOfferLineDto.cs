using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    public class SaveVehicleOfferLineDto
    {
        public int Seq { get; set; }
        public DateTime ServiceDate { get; set; }
        public TimeSpan WindowStart { get; set; }
        public TimeSpan WindowEnd { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }

        // ctor vacío requerido por los serializers
        public SaveVehicleOfferLineDto()
        { }

        public SaveVehicleOfferLineDto(
            int seq,
            DateTime serviceDate,
            TimeSpan windowStart,
            TimeSpan windowEnd,
            decimal price,
            string? notes = null)
        {
            Seq = seq;
            ServiceDate = serviceDate;
            WindowStart = windowStart;
            WindowEnd = windowEnd;
            Price = price;
            Notes = notes;
        }
    }
}