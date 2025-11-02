using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    public class VehiclePickDto
    {
        public int Id { get; set; }
        public string Plate { get; set; } = string.Empty;
        public string? CapacityTonnageLabel { get; set; } // p.ej. "3.5T"
    }
}