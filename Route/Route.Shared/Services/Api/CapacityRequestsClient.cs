using Route.Shared.DTOs;
using Route.Shared.Entities;
using Route.Shared.Enums;
using Route.Shared.Responses;
using Route.Shared.Services.Api.Core;
using System.Net.Http.Json;

namespace Route.Shared.Services.Api
{
    public class CapacityRequestsClient : ApiClientBase
    {
        public CapacityRequestsClient(HttpClient http) : base(http)
        {
        }

        public async Task<PagedResult<CapacityRequest>> GetPagedAsync(
            PaginationDTO p, CapacityReqStatus? status = null, int? providerId = null, bool? visibleForProvider = null,
            DateTime? fromServiceDate = null, DateTime? toServiceDate = null)
        {
            var q = BuildQuery(
                ("page", p.Page.ToString()),
                ("recordsNumber", p.RecordsNumber.ToString()),
                ("term", p.Term),
                ("sortBy", p.SortBy),
                ("sortDir", p.SortDir),
                ("status", status?.ToString()),
                ("providerId", providerId?.ToString()),
                ("visibleForProvider", visibleForProvider?.ToString().ToLower()),
                ("fromServiceDate", fromServiceDate?.ToString("yyyy-MM-dd")),
                ("toServiceDate", toServiceDate?.ToString("yyyy-MM-dd"))
            );
            return await Http.GetFromJsonAsync<PagedResult<CapacityRequest>>($"api/capacityrequests/paged{q}")
                   ?? new PagedResult<CapacityRequest>();
        }
    }
}