using Route.Shared.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Route.Shared.Entities
{
    //public class Vehicle : IEntityWithId
    //{
    //    public int Id { get; set; }
    //    public int ProviderId { get; set; }
    //    public Provider Provider { get; set; } = default!;
    //    public string Plate { get; set; } = string.Empty;
    //    public string? Model { get; set; }
    //    public string? Brand { get; set; }
    //    public double CapacityKg { get; set; }
    //    public double CapacityVolM3 { get; set; }
    //    public int Seats { get; set; } = 2;
    //    public string? Type { get; set; }   // van, truck, moto
    //    public bool IsActive { get; set; } = true;
    //    public string? CapacityTonnageLabel { get; set; }
    //    public ICollection<RoutePlan> Routes { get; set; } = new List<RoutePlan>();
    //}

    public class Vehicle : IEntityWithId
    {
        public int Id { get; set; }

        public int ProviderId { get; set; }

        // 👇 Navegación opcional y no serializada
        [JsonIgnore]
        public Provider? Provider { get; set; }

        public string Plate { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Brand { get; set; }
        public double CapacityKg { get; set; }
        public double CapacityVolM3 { get; set; }
        public int Seats { get; set; } = 2;
        public string? Type { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CapacityTonnageLabel { get; set; }
        public ICollection<RoutePlan> Routes { get; set; } = new List<RoutePlan>();
    }
}