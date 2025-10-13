using Microsoft.EntityFrameworkCore;
using Route.Shared.Entities;
using Route.Shared.Enums;

namespace Route.Backend.Data
{
    public class SeedDb
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<SeedDb> _logger;
        private readonly IConfiguration _configuration;

        public SeedDb(DataContext dataContext, ILogger<SeedDb> logger, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Ejecuta migraciones y, si <paramref name="resetDatabase"/> es true, limpia y vuelve a poblar.
        /// Si <paramref name="resetDatabase"/> es false, el proceso es idempotente (no duplica datos).
        /// </summary>
        public async Task SeedAsync(bool resetDatabase, CancellationToken cancellationToken = default)
        {
            await _dataContext.Database.MigrateAsync(cancellationToken);

            if (resetDatabase)
                await ClearAllAsync(cancellationToken);

            await UpsertProvidersAsync(cancellationToken);
            await UpsertVehiclesAsync(cancellationToken);
            await UpsertOrdersAsync(cancellationToken);
            await UpsertCapacityRequestsAsync(cancellationToken);
            await UpsertVehicleOffersAsync(cancellationToken);
            await UpsertRoutePlanDemoAsync(cancellationToken);

            _logger.LogInformation(
                "Seed OK: Providers={Providers} Vehicles={Vehicles} Orders={Orders} CapacityRequests={CapacityRequests} Offers={Offers} Routes={Routes}",
                await _dataContext.Providers.CountAsync(cancellationToken),
                await _dataContext.Vehicles.CountAsync(cancellationToken),
                await _dataContext.Orders.CountAsync(cancellationToken),
                await _dataContext.CapacityRequests.CountAsync(cancellationToken),
                await _dataContext.VehicleOffers.CountAsync(cancellationToken),
                await _dataContext.RoutePlans.CountAsync(cancellationToken)
            );
        }

        /// <summary>
        /// Overload que toma la bandera desde configuración: "Seed:Reset".
        /// </summary>
        public Task SeedAsync(CancellationToken cancellationToken = default) =>
            SeedAsync(resetDatabase: _configuration.GetValue("Seed:Reset", false), cancellationToken);

        // --------------------------------------------------------------------
        // CLEAR (borrado en orden correcto por FK con Restrict)
        // --------------------------------------------------------------------
        private async Task ClearAllAsync(CancellationToken cancellationToken)
        {
            await using var transaction = await _dataContext.Database.BeginTransactionAsync(cancellationToken);

            await _dataContext.RouteOrders.ExecuteDeleteAsync(cancellationToken);
            await _dataContext.RoutePlans.ExecuteDeleteAsync(cancellationToken);
            await _dataContext.VehicleOffers.ExecuteDeleteAsync(cancellationToken);
            await _dataContext.CapacityRequests.ExecuteDeleteAsync(cancellationToken);
            await _dataContext.Orders.ExecuteDeleteAsync(cancellationToken);
            await _dataContext.Vehicles.ExecuteDeleteAsync(cancellationToken);
            await _dataContext.Providers.ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Seed: base de datos limpiada (resetDatabase=true).");
        }

        // --------------------------------------------------------------------
        // UPSERTS
        // --------------------------------------------------------------------
        private async Task UpsertProvidersAsync(CancellationToken cancellationToken)
        {
            var providerSeedList = new[]
            {
            new Provider {
                Name="Transporte Lima SAC", TaxId="20123456789",
                ContactName="María Torres", Phone="(01) 555-0101",
                Email="contacto@tlima.pe", Address="Av. República 123, Lima", IsActive=true
            },
            new Provider {
                Name="Logística Andina SRL", TaxId="20654321098",
                ContactName="Luis Alvarado", Phone="(01) 555-0202",
                Email="ventas@landina.pe", Address="Av. Industrial 456, Lima", IsActive=true
            }
        };

            foreach (var provider in providerSeedList)
            {
                var existingProvider = await _dataContext.Providers
                    .FirstOrDefaultAsync(x => x.TaxId == provider.TaxId, cancellationToken);

                if (existingProvider is null)
                {
                    _dataContext.Providers.Add(provider);
                }
                else
                {
                    existingProvider.Name = provider.Name;
                    existingProvider.ContactName = provider.ContactName;
                    existingProvider.Phone = provider.Phone;
                    existingProvider.Email = provider.Email;
                    existingProvider.Address = provider.Address;
                    existingProvider.IsActive = provider.IsActive;
                }
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertVehiclesAsync(CancellationToken cancellationToken)
        {
            var providerLima = await _dataContext.Providers.FirstAsync(x => x.TaxId == "20123456789", cancellationToken);
            var providerAndina = await _dataContext.Providers.FirstAsync(x => x.TaxId == "20654321098", cancellationToken);

            var vehicleSeedList = new[]
            {
            new Vehicle { ProviderId = providerLima.Id,   Plate = "ADL801", Brand="Hyundai", Model="H100",
                          CapacityKg=1500, CapacityVolM3=12, Seats=3, Type="van",   IsActive=true, CapacityTonnageLabel="1.5T" },

            new Vehicle { ProviderId = providerLima.Id,   Plate = "PAN1",   Brand="Toyota",  Model="Hiace",
                          CapacityKg=1200, CapacityVolM3=10, Seats=3, Type="van",   IsActive=true, CapacityTonnageLabel="1.2T" },

            new Vehicle { ProviderId = providerAndina.Id, Plate = "VOL456", Brand="Volvo",   Model="VM",
                          CapacityKg=3500, CapacityVolM3=24, Seats=2, Type="truck", IsActive=true, CapacityTonnageLabel="3.5T" }
        };

            foreach (var vehicle in vehicleSeedList)
            {
                var existingVehicle = await _dataContext.Vehicles
                    .FirstOrDefaultAsync(x => x.Plate == vehicle.Plate, cancellationToken);

                if (existingVehicle is null)
                {
                    _dataContext.Vehicles.Add(vehicle);
                }
                else
                {
                    existingVehicle.ProviderId = vehicle.ProviderId;
                    existingVehicle.Brand = vehicle.Brand;
                    existingVehicle.Model = vehicle.Model;
                    existingVehicle.CapacityKg = vehicle.CapacityKg;
                    existingVehicle.CapacityVolM3 = vehicle.CapacityVolM3;
                    existingVehicle.Seats = vehicle.Seats;
                    existingVehicle.Type = vehicle.Type;
                    existingVehicle.IsActive = vehicle.IsActive;
                    existingVehicle.CapacityTonnageLabel = vehicle.CapacityTonnageLabel;
                }
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        private static decimal RoundDecimal(double value, int decimals) =>
            Math.Round((decimal)value, decimals);

        private async Task UpsertOrdersAsync(CancellationToken cancellationToken)
        {
            var random = new Random(7);
            var serviceDateToday = DateTime.Today;

            var districtBaseCoordinates = new (string District, double Latitude, double Longitude)[]
            {
            ("Miraflores", -12.121, -77.030), ("San Isidro", -12.097, -77.037),
            ("Santiago de Surco", -12.149, -76.990), ("San Borja", -12.104, -76.999),
            ("La Molina", -12.089, -76.946), ("Lince", -12.090, -77.037),
            ("San Miguel", -12.075, -77.095), ("Callao", -12.054, -77.118),
            ("Ate", -12.042, -76.940), ("SJL", -12.000, -76.990),
            ("Villa El Salvador", -12.209, -76.941),
            };

            var scheduledDates = new[]
            {
            serviceDateToday, serviceDateToday, serviceDateToday,
            serviceDateToday.AddDays(1), serviceDateToday.AddDays(2)
        };

            string[] paymentMethods = { "CONTADO", "CREDITO", "LETRAS" };

            double RandomGeographicJitter() => (random.NextDouble() - 0.5) * 0.01; // +/- 0.005

            string NextRandomTaxId() => $"{random.Next(10, 99)}{random.Next(100000000, 999999999)}";

            var ordersToUpsert = new List<Order>();
            int totalOrdersToCreate = 60;

            for (int orderIndex = 0; orderIndex < totalOrdersToCreate; orderIndex++)
            {
                var districtBase = districtBaseCoordinates[random.Next(districtBaseCoordinates.Length)];
                var latitude = RoundDecimal(districtBase.Latitude + RandomGeographicJitter(), 6);
                var longitude = RoundDecimal(districtBase.Longitude + RandomGeographicJitter(), 6);
                var scheduledDate = scheduledDates[random.Next(scheduledDates.Length)];

                var packages = random.Next(1, 8);
                var weightKg = RoundDecimal(packages * (5 + random.NextDouble() * 8), 2);
                var volumeM3 = RoundDecimal(packages * (0.025 + random.NextDouble() * 0.04), 3);
                var amountTotal = RoundDecimal(packages * (50 + random.NextDouble() * 200), 2);

                var externalOrderNo = $"PO-{1000 + orderIndex}";

                ordersToUpsert.Add(new Order
                {
                    ExternalOrderNo = externalOrderNo,
                    CustomerName = $"Cliente {orderIndex + 1}",
                    CustomerTaxId = NextRandomTaxId(),
                    Address = $"Av. Ejemplo {100 + orderIndex}",
                    District = districtBase.District,
                    Province = "Lima",
                    Department = "Lima",
                    WeightKg = weightKg,
                    VolumeM3 = volumeM3,
                    Packages = packages,
                    AmountTotal = amountTotal,
                    PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                    Latitude = latitude,
                    Longitude = longitude,
                    BillingDate = null,
                    ScheduledDate = scheduledDate,
                    Status = OrderStatus.Pending
                });
            }

            foreach (var orderCandidate in ordersToUpsert)
            {
                var existingOrder = await _dataContext.Orders
                    .FirstOrDefaultAsync(x => x.ExternalOrderNo == orderCandidate.ExternalOrderNo, cancellationToken);

                if (existingOrder is null)
                {
                    _dataContext.Orders.Add(orderCandidate);
                }
                else
                {
                    // Actualizamos campos “de negocio”
                    existingOrder.CustomerName = orderCandidate.CustomerName;
                    existingOrder.Address = orderCandidate.Address;
                    existingOrder.District = orderCandidate.District;
                    existingOrder.WeightKg = orderCandidate.WeightKg;
                    existingOrder.VolumeM3 = orderCandidate.VolumeM3;
                    existingOrder.Packages = orderCandidate.Packages;
                    existingOrder.AmountTotal = orderCandidate.AmountTotal;
                    existingOrder.PaymentMethod = orderCandidate.PaymentMethod;
                    existingOrder.Latitude = orderCandidate.Latitude;
                    existingOrder.Longitude = orderCandidate.Longitude;
                    existingOrder.ScheduledDate = orderCandidate.ScheduledDate;
                    // Mantenemos el estado actual si ya cambió durante pruebas.
                }
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertCapacityRequestsAsync(CancellationToken cancellationToken)
        {
            var serviceDateToday = DateTime.Today;
            var serviceDateTomorrow = serviceDateToday.AddDays(1);

            var capacityRequestSeedList = new[]
            {
            new CapacityRequest {
                ServiceDate = serviceDateToday, Zone = "Lima Centro",
                DemandWeightKg = 2500, DemandVolumeM3 = 18, DemandStops = 15,
                WindowStart = new TimeSpan(8,0,0), WindowEnd = new TimeSpan(18,0,0),
                Status = CapacityReqStatus.Open, OnlyTargetProvider = false, CreatedBy="seed"
            },
            new CapacityRequest {
                ServiceDate = serviceDateToday, Zone = "Lima Este",
                DemandWeightKg = 1800, DemandVolumeM3 = 12, DemandStops = 10,
                WindowStart = new TimeSpan(8,0,0), WindowEnd = new TimeSpan(18,0,0),
                Status = CapacityReqStatus.Open, OnlyTargetProvider = false, CreatedBy="seed"
            },
            new CapacityRequest {
                ServiceDate = serviceDateTomorrow, Zone = "Lima Sur",
                DemandWeightKg = 2200, DemandVolumeM3 = 16, DemandStops = 12,
                WindowStart = new TimeSpan(8,0,0), WindowEnd = new TimeSpan(18,0,0),
                Status = CapacityReqStatus.Open, OnlyTargetProvider = false, CreatedBy="seed"
            }
        };

            foreach (var capacityRequest in capacityRequestSeedList)
            {
                var existingCapacityRequest = await _dataContext.CapacityRequests
                    .FirstOrDefaultAsync(
                        x => x.ServiceDate == capacityRequest.ServiceDate && x.Zone == capacityRequest.Zone,
                        cancellationToken);

                if (existingCapacityRequest is null)
                {
                    _dataContext.CapacityRequests.Add(capacityRequest);
                }
                else
                {
                    existingCapacityRequest.DemandWeightKg = capacityRequest.DemandWeightKg;
                    existingCapacityRequest.DemandVolumeM3 = capacityRequest.DemandVolumeM3;
                    existingCapacityRequest.DemandStops = capacityRequest.DemandStops;
                    existingCapacityRequest.WindowStart = capacityRequest.WindowStart;
                    existingCapacityRequest.WindowEnd = capacityRequest.WindowEnd;
                    existingCapacityRequest.Status = capacityRequest.Status;
                    existingCapacityRequest.OnlyTargetProvider = capacityRequest.OnlyTargetProvider;
                }
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertVehicleOffersAsync(CancellationToken cancellationToken)
        {
            var serviceDateToday = DateTime.Today;

            var capacityRequestToday = await _dataContext.CapacityRequests
                .Where(x => x.ServiceDate == serviceDateToday)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (capacityRequestToday is null)
                return;

            var allVehicles = await _dataContext.Vehicles.AsNoTracking().ToListAsync(cancellationToken);
            if (allVehicles.Count == 0)
                return;

            var random = new Random(3);

            foreach (var vehicle in allVehicles)
            {
                var offeredWeightKg = Math.Min(vehicle.CapacityKg, 2000 + random.Next(0, 800));
                var offeredVolumeM3 = Math.Min(vehicle.CapacityVolM3, 10 + random.Next(0, 8));

                var existingOffer = await _dataContext.VehicleOffers
                    .FirstOrDefaultAsync(
                        x => x.CapacityRequestId == capacityRequestToday.Id && x.VehicleId == vehicle.Id,
                        cancellationToken);

                if (existingOffer is null)
                {
                    _dataContext.VehicleOffers.Add(new VehicleOffer
                    {
                        CapacityRequestId = capacityRequestToday.Id,
                        ProviderId = vehicle.ProviderId,
                        VehicleId = vehicle.Id,
                        OfferedWeightKg = offeredWeightKg,
                        OfferedVolumeM3 = offeredVolumeM3,
                        Price = (decimal)(600 + random.Next(0, 500)),
                        Currency = "PEN",
                        Status = VehicleOfferStatus.Draft,
                        Notes = "Oferta automática (seed)",
                        DecisionAt = null,
                        DecidedBy = null
                    });
                }
                else
                {
                    existingOffer.OfferedWeightKg = offeredWeightKg;
                    existingOffer.OfferedVolumeM3 = offeredVolumeM3;
                    existingOffer.Price = (decimal)(600 + random.Next(0, 500));
                    existingOffer.Status = VehicleOfferStatus.Draft;
                    existingOffer.Notes = "Oferta automática (seed)";
                    existingOffer.DecisionAt = null;
                    existingOffer.DecidedBy = null;
                }
            }

            // Garantiza al menos una oferta aceptada para construir la ruta de demostración.
            var firstOfferForToday = await _dataContext.VehicleOffers
                .Where(x => x.CapacityRequestId == capacityRequestToday.Id)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (firstOfferForToday is not null)
                firstOfferForToday.Status = VehicleOfferStatus.Accepted;

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertRoutePlanDemoAsync(CancellationToken cancellationToken)
        {
            var serviceDateToday = DateTime.Today;

            var acceptedOffer = await _dataContext.VehicleOffers
                .Include(o => o.Vehicle)
                .Where(o => o.Status == VehicleOfferStatus.Accepted)
                .OrderBy(o => o.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (acceptedOffer is null)
                return;

            var selectedVehicle = acceptedOffer.Vehicle;
            var routeCode = $"{selectedVehicle.Plate}-01";

            var existingRoutePlan = await _dataContext.RoutePlans
                .FirstOrDefaultAsync(r => r.ServiceDate == serviceDateToday && r.Code == routeCode, cancellationToken);

            if (existingRoutePlan is null)
            {
                existingRoutePlan = new RoutePlan
                {
                    ServiceDate = serviceDateToday,
                    VehicleId = selectedVehicle.Id,
                    ProviderId = selectedVehicle.ProviderId,
                    Code = routeCode,
                    Status = RouteStatus.Planned,
                    StartTime = serviceDateToday.AddHours(9),
                    EndTime = serviceDateToday.AddHours(12),
                    DurationMin = 180,
                    DistanceKm = 22.5,
                    ColorHex = "#1b5cff"
                };

                _dataContext.RoutePlans.Add(existingRoutePlan);
                await _dataContext.SaveChangesAsync(cancellationToken);
            }

            // Asegura 5 pedidos asignados a la ruta de demostración
            var alreadyAssignedCount = await _dataContext.RouteOrders
                .Where(ro => ro.RouteId == existingRoutePlan.Id)
                .CountAsync(cancellationToken);

            if (alreadyAssignedCount < 5)
            {
                int pendingToAssign = 5 - alreadyAssignedCount;

                var availableOrders = await _dataContext.Orders
                    .Where(o => o.ScheduledDate == serviceDateToday &&
                                !_dataContext.RouteOrders.Any(ro => ro.OrderId == o.Id))
                    .OrderBy(o => o.Id)
                    .Take(pendingToAssign)
                    .ToListAsync(cancellationToken);

                int nextSequence = alreadyAssignedCount;

                foreach (var order in availableOrders)
                {
                    _dataContext.RouteOrders.Add(new RouteOrder
                    {
                        RouteId = existingRoutePlan.Id,
                        OrderId = order.Id,
                        StopSequence = ++nextSequence,
                        ETA = serviceDateToday.AddHours(9).AddMinutes(15 * nextSequence),
                        DeliveryStatus = DeliveryStatus.Pending
                    });

                    order.Status = OrderStatus.Assigned;
                }

                await _dataContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}