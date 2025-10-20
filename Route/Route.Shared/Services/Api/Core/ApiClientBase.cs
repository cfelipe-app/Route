using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Services.Api.Core
{
    public abstract class ApiClientBase : IApiClient
    {
        protected ApiClientBase(HttpClient http) => Http = http;

        public HttpClient Http { get; }

        protected static string BuildQuery(params (string key, string? val)[] parts)
        {
            var sb = new System.Text.StringBuilder("?");
            bool first = true;
            foreach (var (k, v) in parts)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                if (!first) sb.Append('&'); first = false;
                sb.Append(Uri.EscapeDataString(k)).Append('=').Append(Uri.EscapeDataString(v!));
            }
            return first ? string.Empty : sb.ToString();
        }

        protected static async Task<string> ReadErrorAsync(HttpResponseMessage r)
            => $"{(int)r.StatusCode} {r.StatusCode}: {await r.Content.ReadAsStringAsync()}";
    }
}