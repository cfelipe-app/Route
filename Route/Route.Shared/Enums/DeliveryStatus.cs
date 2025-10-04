using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Enums
{
    public enum DeliveryStatus
    {
        [Description("Pendiente")]
        Pending = 0,

        EnRoute = 1,

        [Description("Entregado")]
        Delivered = 2,

        [Description("Fallida")]
        Failed = 3
    }
}