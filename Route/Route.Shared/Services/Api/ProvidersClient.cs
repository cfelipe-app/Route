using Microsoft.AspNetCore.WebUtilities;
using Route.Shared.Entities;
using Route.Shared.Responses;
using Route.Shared.DTOs;
using System.Net.Http.Json;

namespace Route.Shared.Services.Api
{
    public class ProvidersClient
    {
        private readonly HttpClient _http;

        public ProvidersClient(HttpClient http) => _http = http;

        public async Task<PagedResult<Provider>?> GetPagedAsync(
            string? term = null, int page = 1, int recordsNumber = 10,
            string? sortBy = "Name", string sortDir = "asc",
            CancellationToken cancellationToken = default)
        {
            var q = new Dictionary<string, string?>
            {
                ["term"] = term,
                ["page"] = page.ToString(),
                ["recordsNumber"] = recordsNumber.ToString(),
                ["sortBy"] = sortBy,
                ["sortDir"] = sortDir
            };
            var url = QueryHelpers.AddQueryString("api/providers/paged", q);
            return await _http.GetFromJsonAsync<PagedResult<Provider>>(url, cancellationToken);
        }

        // ---- Crear / Actualizar devolviendo el error del backend ----
        public async Task<(bool ok, string? error)> CreateRawAsync(Provider provider, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/providers", provider, ct);
            if (resp.IsSuccessStatusCode) return (true, null);
            return (false, await ReadBody(resp));
        }

        public async Task<(bool ok, string? error)> UpdateRawAsync(Provider provider, CancellationToken ct = default)
        {
            var resp = await _http.PutAsJsonAsync($"api/providers/{provider.Id}", provider, ct);
            if (resp.IsSuccessStatusCode) return (true, null);
            return (false, await ReadBody(resp));
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
            => (await _http.DeleteAsync($"api/providers/{id}", ct)).IsSuccessStatusCode;

        // Wrappers “bool” si los prefieres:
        public async Task<bool> CreateAsync(Provider p, CancellationToken ct = default)
            => (await CreateRawAsync(p, ct)).ok;

        public async Task<bool> UpdateAsync(Provider p, CancellationToken ct = default)
            => (await UpdateRawAsync(p, ct)).ok;

        private static async Task<string> ReadBody(HttpResponseMessage resp)
        {
            try { return $"[{(int)resp.StatusCode}] {resp.ReasonPhrase}\n" + (await resp.Content.ReadAsStringAsync()); }
            catch { return $"[{(int)resp.StatusCode}] {resp.ReasonPhrase}"; }
        }
    }
}