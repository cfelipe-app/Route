using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Enums
{
    public enum CapacityReqStatus
    {
        [Description("Abierta")]
        Open = 0,             // Creada. Puedes usarla para "borrador" o "lista para publicar"

        Quoted = 1,           // En cotización: recibiendo ofertas (equivale a Published)

        Awarded = 2,          // Adjudicada (cuando al menos una oferta fue aceptada)

        [Description("Cerrada")]
        Closed = 3,           // Cerrada (demanda totalmente cubierta o se decide cerrar)

        [Description("Parcialmente adjudicada")]
        PartiallyAwarded = 4, // (NUEVO) Aceptaste algo pero aún no cubre todo

        [Description("Vencida")]
        Expired = 5,          // (NUEVO) Ya no aplica por fecha/ventana

        [Description("Cancelada")]
        Cancelled = 9         // Cancelada explícitamente
    }
}