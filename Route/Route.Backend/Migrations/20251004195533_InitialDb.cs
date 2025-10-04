using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Route.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalOrderNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CustomerTaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VolumeM3 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Packages = table.Column<int>(type: "int", nullable: false),
                    AmountTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    InvoiceDoc = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuideDoc = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    GuideDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransportRuc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransportName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DeliveryDeptGuide = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CapacityRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    OnlyTargetProvider = table.Column<bool>(type: "bit", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    DemandWeightKg = table.Column<double>(type: "float", nullable: false),
                    DemandVolumeM3 = table.Column<double>(type: "float", nullable: false),
                    DemandStops = table.Column<int>(type: "int", nullable: false),
                    WindowStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    WindowEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapacityRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapacityRequests_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    Plate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CapacityKg = table.Column<double>(type: "float", nullable: false),
                    CapacityVolM3 = table.Column<double>(type: "float", nullable: false),
                    Seats = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CapacityTonnageLabel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoutePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DistanceKm = table.Column<double>(type: "float", nullable: false),
                    DurationMin = table.Column<double>(type: "float", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    DriverUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutePlans_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoutePlans_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CapacityRequestId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    OfferedWeightKg = table.Column<double>(type: "float", nullable: false),
                    OfferedVolumeM3 = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DecisionAt = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleOffers_CapacityRequests_CapacityRequestId",
                        column: x => x.CapacityRequestId,
                        principalTable: "CapacityRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleOffers_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleOffers_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteOrders",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    StopSequence = table.Column<int>(type: "int", nullable: false),
                    ETA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ETD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProofPhotoUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteOrders", x => new { x.RouteId, x.OrderId });
                    table.ForeignKey(
                        name: "FK_RouteOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteOrders_RoutePlans_RouteId",
                        column: x => x.RouteId,
                        principalTable: "RoutePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapacityRequests_ProviderId",
                table: "CapacityRequests",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_CapacityRequests_ServiceDate_ProviderId",
                table: "CapacityRequests",
                columns: new[] { "ServiceDate", "ProviderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_InvoiceDoc_GuideDoc",
                table: "Orders",
                columns: new[] { "InvoiceDoc", "GuideDoc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Latitude_Longitude",
                table: "Orders",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ScheduledDate_Status",
                table: "Orders",
                columns: new[] { "ScheduledDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Providers_TaxId",
                table: "Providers",
                column: "TaxId",
                unique: true,
                filter: "[TaxId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RouteOrders_OrderId",
                table: "RouteOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteOrders_RouteId_StopSequence",
                table: "RouteOrders",
                columns: new[] { "RouteId", "StopSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoutePlans_ProviderId",
                table: "RoutePlans",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutePlans_ServiceDate",
                table: "RoutePlans",
                column: "ServiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_RoutePlans_ServiceDate_Code",
                table: "RoutePlans",
                columns: new[] { "ServiceDate", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoutePlans_VehicleId",
                table: "RoutePlans",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOffers_CapacityRequestId_VehicleId",
                table: "VehicleOffers",
                columns: new[] { "CapacityRequestId", "VehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOffers_ProviderId",
                table: "VehicleOffers",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOffers_VehicleId",
                table: "VehicleOffers",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Plate",
                table: "Vehicles",
                column: "Plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ProviderId",
                table: "Vehicles",
                column: "ProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteOrders");

            migrationBuilder.DropTable(
                name: "VehicleOffers");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "RoutePlans");

            migrationBuilder.DropTable(
                name: "CapacityRequests");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Providers");
        }
    }
}
