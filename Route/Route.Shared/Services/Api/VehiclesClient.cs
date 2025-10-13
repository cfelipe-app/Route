using Microsoft.AspNetCore.WebUtilities;
using Route.Shared.Entities;
using Route.Shared.Responses;
using System.Net.Http.Json;

namespace Route.Shared.Services.Api
{
    public sealed class VehiclesClient
    {
        private readonly HttpClient _httpClient;

        public VehiclesClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<PagedResult<Vehicle>?> GetPagedAsync(
            string? searchTerm = null,
            int page = 1,
            int recordsNumber = 10,
            string? sortBy = "Plate",
            string sortDirection = "asc",
            int? providerId = null,
            bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string?>
            {
                ["term"] = searchTerm,
                ["page"] = page.ToString(),
                ["recordsNumber"] = recordsNumber.ToString(),
                ["sortBy"] = string.IsNullOrWhiteSpace(sortBy) ? "Plate" : sortBy,
                ["sortDir"] = string.IsNullOrWhiteSpace(sortDirection) ? "asc" : sortDirection
            };

            if (providerId.HasValue) query["providerId"] = providerId.Value.ToString();
            if (isActive.HasValue) query["isActive"] = isActive.Value.ToString();

            string url = QueryHelpers.AddQueryString("api/vehicles/paged", query);
            return await _httpClient.GetFromJsonAsync<PagedResult<Vehicle>>(url, cancellationToken);
        }

        public Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _httpClient.GetFromJsonAsync<Vehicle>($"api/vehicles/{id}", cancellationToken);

        public async Task<(bool ok, string? error, int status)> CreateAsync(
            Vehicle vehicle,
            CancellationToken cancellationToken = default)
        {
            Vehicle payload = PrepareForSend(vehicle, isCreate: true);
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/vehicles", payload, cancellationToken);

            if (response.IsSuccessStatusCode) return (true, null, (int)response.StatusCode);
            return (false, await ReadErrorAsync(response, cancellationToken), (int)response.StatusCode);
        }

        public async Task<(bool ok, string? error, int status)> UpdateAsync(
            Vehicle vehicle,
            CancellationToken cancellationToken = default)
        {
            Vehicle payload = PrepareForSend(vehicle, isCreate: false);
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/vehicles/{vehicle.Id}", payload, cancellationToken);

            if (response.IsSuccessStatusCode) return (true, null, (int)response.StatusCode);
            return (false, await ReadErrorAsync(response, cancellationToken), (int)response.StatusCode);
        }

        public async Task<(bool ok, string? error, int status)> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/vehicles/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null, (int)response.StatusCode);
            return (false, await ReadErrorAsync(response, cancellationToken), (int)response.StatusCode);
        }

        private static Vehicle PrepareForSend(Vehicle vehicle, bool isCreate)
        {
            return new Vehicle
            {
                Id = isCreate ? 0 : vehicle.Id,
                ProviderId = vehicle.ProviderId,
                Plate = (vehicle.Plate ?? string.Empty).Trim().ToUpperInvariant(),
                Brand = string.IsNullOrWhiteSpace(vehicle.Brand) ? null : vehicle.Brand.Trim(),
                Model = string.IsNullOrWhiteSpace(vehicle.Model) ? null : vehicle.Model.Trim(),
                CapacityKg = vehicle.CapacityKg,
                CapacityVolM3 = vehicle.CapacityVolM3,
                Seats = vehicle.Seats,
                Type = string.IsNullOrWhiteSpace(vehicle.Type) ? null : vehicle.Type.Trim(),
                CapacityTonnageLabel = string.IsNullOrWhiteSpace(vehicle.CapacityTonnageLabel)
                    ? null : vehicle.CapacityTonnageLabel.Trim(),
                IsActive = vehicle.IsActive
            };
        }

        private static async Task<string?> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(body))
                    return $"[{(int)response.StatusCode}] {body}";
            }
            catch { /* ignore */ }

            return $"[{(int)response.StatusCode}] {response.ReasonPhrase}";
        }
    }
}