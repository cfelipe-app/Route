using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Enums
{
    public record EnumLookup<TEnum>(TEnum Value, string Label);
}