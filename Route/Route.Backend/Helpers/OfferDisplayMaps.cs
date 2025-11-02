using Route.Shared.Enums;

namespace Route.Backend.Helpers
{
    //public static class OfferDisplayMaps
    //{
    //    public static string ToDisplay(this VehicleOfferStatus s) => s switch
    //    {
    //        VehicleOfferStatus.Draft => "Borrador",
    //        VehicleOfferStatus.Sent => "Enviada",
    //        VehicleOfferStatus.Accepted => "Aceptada",
    //        VehicleOfferStatus.Rejected => "Rechazada",
    //        VehicleOfferStatus.Withdrawn => "Retirada",
    //        VehicleOfferStatus.Expired => "Vencida",
    //        _ => s.ToString()
    //    };

    //    public static string ToDisplay(this PriceMode m) => m switch
    //    {
    //        PriceMode.PerVehicle => "Por vehículo",
    //        PriceMode.Total => "Total",
    //        _ => m.ToString()
    //    };
    //}

    public static class OfferDisplayMaps
    {
        public static string ToDisplay(this VehicleOfferStatus s) => s switch
        {
            VehicleOfferStatus.Draft => "Borrador",
            VehicleOfferStatus.Sent => "Enviada",
            VehicleOfferStatus.Accepted => "Aceptada",
            VehicleOfferStatus.Rejected => "Rechazada",
            VehicleOfferStatus.Withdrawn => "Retirada",
            VehicleOfferStatus.Expired => "Vencida",
            _ => s.ToString()
        };

        public static string ToDisplay(this PriceMode m) => m switch
        {
            PriceMode.PerVehicle => "Por vehículo",
            PriceMode.Total => "Total",
            _ => m.ToString()
        };
    }
}