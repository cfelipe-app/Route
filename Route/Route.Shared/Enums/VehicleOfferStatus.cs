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
        [Description("Pendiente")]
        Draft = 0,          // Borrador (aún no visible)

        [Description("Enviado")]
        Sent = 1,           // Propuesta publicada/emitida al planner

        [Description("Aceptado")]
        Accepted = 2,       // Adjudicada (total o parcial)

        [Description("Rechazado")]
        Rejected = 3,       // Rechazada por el planner

        [Description("Retirado")]
        Withdrawn = 4,      // (NUEVO) El proveedor retiró su oferta antes de decisión

        [Description("Vencido")]
        Expired = 5         // (NUEVO) Pasó la fecha de vigencia (ValidUntil < UtcNow)
    }
}