using Route.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    // -----------------------------------------------------------
    // Decide (Aceptar / Rechazar / otros estados si deseas)
    // -----------------------------------------------------------
    public class DecideOfferDto
    {
        public VehicleOfferStatus Status { get; set; } // Accepted / Rejected / Withdrawn, etc.
        public string? DecidedBy { get; set; }
    }
}