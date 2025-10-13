using Route.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    /// <summary>
    /// DTO para actualizar el estado de CapacityRequest.
    /// </summary>
    public class UpdateCapacityRequestStatusDto
    {
        public CapacityReqStatus Status { get; set; }
    }
}