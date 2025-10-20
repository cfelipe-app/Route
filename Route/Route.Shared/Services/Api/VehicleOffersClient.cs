using Route.Shared.DTOs;
using Route.Shared.Entities;
using Route.Shared.Enums;
using Route.Shared.Responses;
using Route.Shared.Services.Api.Core;
using System.Net.Http.Json;

namespace Route.Shared.Services.Api
{
    public class VehicleOffersClient : ApiClientBase
    {
        public VehicleOffersClient(HttpClient http) : base(http)
        {
        }

        public record SaveVehicleOfferDto(
            int CapacityRequestId, int ProviderId, int? VehicleId, int Quantity,
            double OfferedWeightKg, double OfferedVolumeM3,
            decimal Price, string Currency, PriceMode PriceMode,
            DateTime? ValidUntil, string? Notes);

        // DEVUELVE el DTO COMPARTIDO
        public async Task<PagedResult<VehicleOfferDto>> GetPagedAsync(
            PaginationDTO p,
            int? capacityRequestId = null, int? providerId = null, int? vehicleId = null,
            VehicleOfferStatus? status = null, DateTime? fromCreated = null, DateTime? toCreated = null,
            bool? visibleForProvider = null)
        {
            var q = BuildQuery(
                ("page", p.Page.ToString()),
                ("recordsNumber", p.RecordsNumber.ToString()),
                ("term", p.Term),
                ("sortBy", p.SortBy),
                ("sortDir", p.SortDir),
                ("capacityRequestId", capacityRequestId?.ToString()),
                ("providerId", providerId?.ToString()),
                ("vehicleId", vehicleId?.ToString()),
                ("status", status?.ToString()),
                ("fromCreated", fromCreated?.ToString("yyyy-MM-dd")),
                ("toCreated", toCreated?.ToString("yyyy-MM-dd")),
                ("visibleForProvider", visibleForProvider?.ToString().ToLower())
            );

            return await Http.GetFromJsonAsync<PagedResult<VehicleOfferDto>>($"api/vehicleoffers/paged{q}")
                   ?? new PagedResult<VehicleOfferDto>();
        }

        public async Task<PagedResult<VehicleOfferDto>> GetByProviderPagedAsync(int providerId, PaginationDTO p)
        {
            var q = BuildQuery(
                ("page", p.Page.ToString()),
                ("recordsNumber", p.RecordsNumber.ToString()),
                ("term", p.Term),
                ("sortBy", p.SortBy),
                ("sortDir", p.SortDir)
            );

            return await Http.GetFromJsonAsync<PagedResult<VehicleOfferDto>>(
                       $"api/vehicleoffers/by-provider/{providerId}{q}")
                   ?? new PagedResult<VehicleOfferDto>();
        }

        public Task<HttpResponseMessage> CreateAsync(SaveVehicleOfferDto dto) =>
            Http.PostAsJsonAsync("api/vehicleoffers/create", dto);

        public Task<HttpResponseMessage> UpdateAsync(int id, SaveVehicleOfferDto dto) =>
            Http.PutAsJsonAsync($"api/vehicleoffers/update/{id}", dto);

        public Task<HttpResponseMessage> DecideAsync(int id, VehicleOfferStatus status, string? decidedBy = null) =>
            Http.PutAsJsonAsync($"api/vehicleoffers/{id}/decide", new { Status = status, DecidedBy = decidedBy });

        // Lookups de estado
        public Task<List<EnumLookup<VehicleOfferStatus>>?> GetStatusLookupsAsync() =>
            Http.GetFromJsonAsync<List<EnumLookup<VehicleOfferStatus>>>("api/vehicleoffers/lookups/status");
    }
}