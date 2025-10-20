using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Services.Api.Core
{
    public interface IApiClient
    {
        HttpClient Http { get; }
    }
}