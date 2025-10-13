using Route.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Shared.Entities
{
    //public class Provider : IEntityWithId, IEntityWithName
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; } = string.Empty;
    //    public string TaxId { get; set; } = string.Empty;
    //    public string? ContactName { get; set; }
    //    public string? Phone { get; set; }
    //    public string? Email { get; set; }
    //    public string? Address { get; set; }
    //    public bool IsActive { get; set; } = true;
    //    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    //    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    //}

    public class Provider : IEntityWithId, IEntityWithName
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        // 🔹 Nuevo: si quieres ver/consolidar los conductores del proveedor
        public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    }
}