using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.DTOs
{
    /// <summary>
    /// DTO de resumen de CapacityRequests para KPIs.
    /// </summary>
    public class CapacityRequestSummaryDto
    {
        public int Total { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
    }
}