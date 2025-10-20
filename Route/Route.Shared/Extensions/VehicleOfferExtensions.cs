using Route.Shared.Entities;
using Route.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Extensions
{
    public static class VehicleOfferExtensions
    {
        public static decimal TotalPrice(this VehicleOffer o) =>
            o.PriceMode == PriceMode.PerVehicle
                ? o.Price * Math.Max(1, o.Quantity)
                : o.Price;
    }
}