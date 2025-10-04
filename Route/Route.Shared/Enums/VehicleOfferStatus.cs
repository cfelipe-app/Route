using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Enums
{
    public enum VehicleOfferStatus
    {
        Draft = 0,

        Sent = 1,

        [Description("Aceptado")]
        Accepted = 2,

        [Description("Rechazado")]
        Rejected = 3
    }
}