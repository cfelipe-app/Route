using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Route.Backend.Helpers;
using Route.Backend.Repositories.Interfaces;
using Route.Backend.UnitsOfWork.Interfaces;
using Route.Shared.DTOs;
using Route.Shared.Entities;
using Route.Shared.Responses;

namespace Route.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : GenericController<Vehicle>
    {
        private readonly IGenericUnitOfWork<Vehicle> _vehicleUnitOfWork;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;

        public VehiclesController(
            IGenericUnitOfWork<Vehicle> vehicleUnitOfWork,
            IGenericRepository<Vehicle> vehicleRepository) : base(vehicleUnitOfWork)
        {
            _vehicleUnitOfWork = vehicleUnitOfWork;
            _vehicleRepository = vehicleRepository;
        }

        // ===================== Paged =====================
        // GET /api/vehicles/paged?term=XYZ&page=1&recordsNumber=10&sortBy=Plate&sortDir=asc&providerId=&isActive=
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<Vehicle>>> GetPaged(
            [FromQuery] PaginationDTO pagination,
            [FromQuery] int? providerId = null,
            [FromQuery] bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pagination.SortBy)) pagination.SortBy = "Plate";
            if (string.IsNullOrWhiteSpace(pagination.SortDir)) pagination.SortDir = "asc";

            IQueryable<Vehicle> query = _vehicleRepository.Query()
                .ApplyFilter(pagination.Term)
                .ApplySearch(pagination.Term, "Plate", "Brand", "Model");

            if (providerId.HasValue) query = query.Where(v => v.ProviderId == providerId.Value);
            if (isActive.HasValue) query = query.Where(v => v.IsActive == isActive.Value);

            var ordered = query.ApplySort(pagination.SortBy!, pagination.SortDir!);

            var total = await ordered.CountAsync(cancellationToken);
            var items = await ordered.Paginate(pagination).ToListAsync(cancellationToken);

            Response.Headers["X-Total-Count"] = total.ToString();

            return Ok(new PagedResult<Vehicle>
            {
                Items = items,
                Page = pagination.Page,
                PageSize = pagination.RecordsNumber,
                Total = total
            });
        }

        // Ruta nombrada para CreatedAtRoute
        [HttpGet("{id:int}", Name = "Vehicles_GetById")]
        public new Task<ActionResult<Vehicle>> GetByIdAsync(int id) => base.GetByIdAsync(id);

        [HttpPost]
        public override async Task<ActionResult<Vehicle>> PostAsync([FromBody] Vehicle vehicle)
        {
            if (!ModelState.IsValid)
            {
                string validationMessage = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(validationMessage);
            }

            Normalize(vehicle);
            vehicle.Id = 0;
            vehicle.Provider = null;
            vehicle.Routes = null;

            bool plateExists = await _vehicleRepository.Query()
                .IgnoreQueryFilters()
                .AnyAsync(v => v.Plate == vehicle.Plate);

            if (plateExists)
                return Conflict("La placa ya está registrada. No puede volver a registrarla.");

            var unitOfWorkResult = await _vehicleUnitOfWork.AddAsync(vehicle);
            if (!unitOfWorkResult.WasSuccess)
                return BadRequest(unitOfWorkResult.Message ?? "No se pudo crear el vehículo.");

            return CreatedAtRoute("Vehicles_GetById", new { id = unitOfWorkResult.Result!.Id }, unitOfWorkResult.Result);
        }

        [HttpPut("{id:int}")]
        public override async Task<IActionResult> PutAsync(int id, [FromBody] Vehicle vehicle)
        {
            if (id != vehicle.Id) return BadRequest("Id mismatch.");

            if (!ModelState.IsValid)
            {
                string validationMessage = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(validationMessage);
            }

            var existingVehicle = await _vehicleRepository.Query()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (existingVehicle is null) return NotFound("No se encontró el vehículo.");

            Normalize(vehicle);
            vehicle.Provider = null;
            vehicle.Routes = null;

            bool plateDuplicate = await _vehicleRepository.Query()
                .IgnoreQueryFilters()
                .AnyAsync(v => v.Id != id && v.Plate == vehicle.Plate);

            if (plateDuplicate)
                return Conflict("La placa ya está registrada. No puede volver a registrarla.");

            var unitOfWorkResult = await _vehicleUnitOfWork.UpdateAsync(vehicle);
            if (!unitOfWorkResult.WasSuccess)
                return BadRequest(unitOfWorkResult.Message ?? "No se pudo actualizar el vehículo.");

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            var unitOfWorkResult = await _vehicleUnitOfWork.DeleteAsync(id);
            return unitOfWorkResult.WasSuccess ? NoContent() : BadRequest(unitOfWorkResult.Message ?? "No se pudo eliminar.");
        }

        private static void Normalize(Vehicle vehicle)
        {
            vehicle.Plate = (vehicle.Plate ?? string.Empty).Trim().ToUpperInvariant();
            vehicle.Brand = string.IsNullOrWhiteSpace(vehicle.Brand) ? null : vehicle.Brand.Trim();
            vehicle.Model = string.IsNullOrWhiteSpace(vehicle.Model) ? null : vehicle.Model.Trim();
            vehicle.Type = string.IsNullOrWhiteSpace(vehicle.Type) ? null : vehicle.Type.Trim();
        }
    }
}