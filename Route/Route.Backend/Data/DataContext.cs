using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Route.Backend.Identity;
using Route.Shared.Entities;
using Route.Shared.Enums;

namespace Route.Backend.Data
{
    public class DataContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Provider> Providers => Set<Provider>();

        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<RoutePlan> RoutePlans => Set<RoutePlan>();
        public DbSet<RouteOrder> RouteOrders => Set<RouteOrder>();
        public DbSet<CapacityRequest> CapacityRequests => Set<CapacityRequest>();
        public DbSet<VehicleOffer> VehicleOffers => Set<VehicleOffer>();
        public DbSet<Driver> Drivers => Set<Driver>();                // <<< NUEVO
        public DbSet<VehicleOfferLine> VehicleOfferLines => Set<VehicleOfferLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= Provider =================
            modelBuilder.Entity<Provider>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(120).IsRequired();
                e.Property(x => x.TaxId).HasMaxLength(20);
                e.Property(x => x.ContactName).HasMaxLength(120);
                e.Property(x => x.Phone).HasMaxLength(50);
                e.Property(x => x.Email).HasMaxLength(120);
                e.Property(x => x.Address).HasMaxLength(200);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasIndex(x => x.TaxId).IsUnique().HasFilter("[TaxId] IS NOT NULL");

                // Filtro global de activos
                e.HasQueryFilter(p => p.IsActive);
            });

            // ================= Vehicle =================
            modelBuilder.Entity<Vehicle>(e =>
            {
                e.Property(v => v.Plate).HasMaxLength(20).IsRequired();
                e.Property(v => v.Model).HasMaxLength(60);
                e.Property(v => v.Brand).HasMaxLength(60);
                e.Property(v => v.Type).HasMaxLength(40);
                e.Property(v => v.CapacityTonnageLabel).HasMaxLength(40);
                e.HasIndex(v => v.Plate).IsUnique();

                e.HasOne(v => v.Provider)
                    .WithMany(p => p.Vehicles)
                    .HasForeignKey(v => v.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Coincidir filtro global del lado dependiente (quita warning 10622)
                e.HasQueryFilter(v => v.IsActive && v.Provider.IsActive);
            });

            // ================= Order =================
            modelBuilder.Entity<Order>(e =>
            {
                // Reglas básicas
                e.Property(o => o.CustomerName).HasMaxLength(150).IsRequired();
                e.Property(o => o.CustomerTaxId).HasMaxLength(20);
                e.Property(o => o.Address).HasMaxLength(220).IsRequired();
                e.Property(o => o.District).HasMaxLength(100);
                e.Property(o => o.Province).HasMaxLength(100);
                e.Property(o => o.Department).HasMaxLength(100);

                // Importes / cantidades
                e.Property(o => o.AmountTotal).HasPrecision(18, 2);
                e.Property(o => o.WeightKg).HasPrecision(18, 2);
                e.Property(o => o.VolumeM3).HasPrecision(18, 2);

                // Documentos / transporte
                e.Property(o => o.InvoiceDoc).HasMaxLength(40);
                e.Property(o => o.GuideDoc).HasMaxLength(40);
                e.Property(o => o.TransportRuc).HasMaxLength(20);
                e.Property(o => o.TransportName).HasMaxLength(120);
                e.Property(o => o.DeliveryDeptGuide).HasMaxLength(120);

                // Timestamps
                e.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.Property(o => o.BillingDate).IsRequired(false); // la propiedad es nullable en la entidad

                // Geo precisión
                e.Property(o => o.Latitude).HasPrecision(9, 6);
                e.Property(o => o.Longitude).HasPrecision(9, 6);

                // Índices útiles
                e.HasIndex(o => new { o.InvoiceDoc, o.GuideDoc });
                e.HasIndex(o => new { o.ScheduledDate, o.Status });
                e.HasIndex(o => new { o.Latitude, o.Longitude });

                // Enum como string
                e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            });

            // ================= RoutePlan =================
            modelBuilder.Entity<RoutePlan>(e =>
            {
                e.Property(r => r.Code).HasMaxLength(30);
                e.Property(r => r.ColorHex).HasMaxLength(7);
                e.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                e.HasOne(r => r.Provider)
                    .WithMany()
                    .HasForeignKey(r => r.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.Vehicle)
                    .WithMany(v => v.Routes)
                    .HasForeignKey(r => r.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                // <<< NUEVO: relación con Driver
                e.HasOne(r => r.Driver)
                    .WithMany(d => d.Routes)
                    .HasForeignKey(r => r.DriverId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Único por fecha+code, permitiendo múltiples NULL en Code (filtro)
                e.HasIndex(r => new { r.ServiceDate, r.Code })
                    .IsUnique()
                    .HasFilter("[Code] IS NOT NULL");

                e.HasIndex(r => r.ServiceDate);
                e.HasIndex(r => new { r.ServiceDate, r.DriverId }); // útil para agenda de conductor

                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            });

            // ================= RouteOrder (PK compuesta) =================
            modelBuilder.Entity<RouteOrder>(e =>
            {
                // PK compuesta RouteId + OrderId
                e.HasKey(ro => new { ro.RouteId, ro.OrderId });

                e.HasOne(ro => ro.Route)
                    .WithMany(r => r.Orders)
                    .HasForeignKey(ro => ro.RouteId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(ro => ro.Order)
                    .WithMany(o => o.RouteOrders)
                    .HasForeignKey(ro => ro.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(ro => ro.ProofPhotoUrl).HasMaxLength(300);
                e.Property(ro => ro.Notes).HasMaxLength(500);

                // Secuencia única por ruta
                e.HasIndex(ro => new { ro.RouteId, ro.StopSequence }).IsUnique();

                e.Property(x => x.DeliveryStatus).HasConversion<string>().HasMaxLength(20);
            });

            // ================= CapacityRequest =================
            modelBuilder.Entity<CapacityRequest>(e =>
            {
                e.Property(cr => cr.Zone).HasMaxLength(80);
                e.Property(cr => cr.CreatedBy).HasMaxLength(80);
                e.Property(cr => cr.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.Property(cr => cr.WindowStart).HasColumnType("time");
                e.Property(cr => cr.WindowEnd).HasColumnType("time");

                e.HasOne(cr => cr.Provider)
                    .WithMany()
                    .HasForeignKey(cr => cr.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices útiles para filtros y visibilidad
                e.HasIndex(cr => cr.ServiceDate);
                e.HasIndex(cr => cr.Status);
                e.HasIndex(cr => new { cr.ServiceDate, cr.ProviderId });
                e.HasIndex(cr => new { cr.ProviderId, cr.OnlyTargetProvider });

                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            });

            // ================= VehicleOffer =================
            modelBuilder.Entity<VehicleOffer>(e =>
            {
                e.ToTable("VehicleOffers");

                e.Property(x => x.Price).HasPrecision(18, 2);
                e.Property(x => x.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("PEN");
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.PriceMode).HasConversion<string>().HasMaxLength(12).HasDefaultValue(PriceMode.PerVehicle);
                e.Property(x => x.Notes).HasMaxLength(500);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.ValidUntil).HasColumnType("datetime2");
                e.Property(x => x.DecisionAt).HasColumnType("datetime2");
                e.Property(x => x.DecidedBy).HasMaxLength(80);

                e.Property(x => x.OfferedWeightKg).HasPrecision(18, 3);
                e.Property(x => x.OfferedVolumeM3).HasPrecision(18, 3);

                e.HasOne(x => x.CapacityRequest)
                    .WithMany(cr => cr.Offers)
                    .HasForeignKey(x => x.CapacityRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Provider)
                    .WithMany()
                    .HasForeignKey(x => x.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Vehicle)
                    .WithMany()
                    .HasForeignKey(x => x.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.ProviderId);
                e.HasIndex(x => new { x.CapacityRequestId, x.Status });

                e.HasIndex(x => new { x.CapacityRequestId, x.VehicleId })
                    .IsUnique()
                    .HasDatabaseName("UX_VehicleOffers_ByRequestVehicle")
                    .HasFilter("[VehicleId] IS NOT NULL");

                e.HasIndex(x => new { x.CapacityRequestId, x.ProviderId })
                    .HasDatabaseName("IX_VehicleOffers_ByRequestProvider_Aggregated")
                    .HasFilter("[VehicleId] IS NULL");

                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_VehicleOffers_Quantity_Positive", "Quantity >= 1");
                    tb.HasCheckConstraint("CK_VehicleOffers_Price_NonNegative", "Price >= 0");
                    tb.HasCheckConstraint("CK_VehicleOffers_WeightsVolumes_NonNegative",
                                          "OfferedWeightKg >= 0 AND OfferedVolumeM3 >= 0");
                });

                // Relación con líneas + cascade explícito
                e.HasMany(o => o.Lines)
                 .WithOne(l => l.Offer)
                 .HasForeignKey(l => l.OfferId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Coincidir filtro global del lado dependiente
                e.HasQueryFilter(vo => vo.Provider.IsActive);
            });

            // ================= VehicleOfferLine =================
            modelBuilder.Entity<VehicleOfferLine>(e =>
            {
                e.ToTable("VehicleOfferLines");
                e.Property(x => x.Seq).IsRequired();
                e.Property(x => x.ServiceDate).IsRequired();
                e.Property(x => x.WindowStart).HasColumnType("time");
                e.Property(x => x.WindowEnd).HasColumnType("time");
                e.Property(x => x.Price).HasPrecision(18, 2);
                e.Property(x => x.Notes).HasMaxLength(300);

                e.HasIndex(x => new { x.OfferId, x.Seq }).IsUnique();

                //Filtro global que “empareja” al del padre (VehicleOffer)
                e.HasQueryFilter(l => l.Offer.Provider.IsActive);
            });

            /// ================= Driver =================
            modelBuilder.Entity<Driver>(e =>
            {
                e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
                e.Property(x => x.DocumentId).HasMaxLength(30);
                e.Property(x => x.Phone).HasMaxLength(50);
                e.Property(x => x.Email).HasMaxLength(120);
                e.Property(x => x.LicenseNumber).HasMaxLength(40);
                e.Property(x => x.LicenseClass).HasMaxLength(20);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.IsActive).HasDefaultValue(true);

                e.HasOne(x => x.Provider)
                    .WithMany(p => p.Drivers)
                    .HasForeignKey(x => x.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.ProviderId, x.DocumentId })
                    .IsUnique()
                    .HasFilter("[DocumentId] IS NOT NULL");

                // Coincidir filtro global del lado dependiente (quita warning 10622)
                e.HasQueryFilter(d => d.IsActive && d.Provider.IsActive);
            });

            // Desactivar cascadas por defecto…
            DisableCascadeDelete(modelBuilder);

            // …pero asegúrate de que Offer → Lines quede en Cascade (por si el método anterior
            // lo cambió). Esto vuelve a establecer Cascade *después* del cambio global:
            modelBuilder.Entity<VehicleOfferLine>()
                .HasOne(l => l.Offer)
                .WithMany(o => o.Lines)
                .HasForeignKey(l => l.OfferId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void DisableCascadeDelete(ModelBuilder modelBuilder)
        {
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}