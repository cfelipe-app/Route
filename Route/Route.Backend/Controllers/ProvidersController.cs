using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Route.Backend.Data;
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
    public class ProvidersController : GenericController<Provider>
    {
        private readonly DataContext _db;
        private readonly IGenericRepository<Provider> _providerRepository;

        public ProvidersController(
            DataContext db,
            IGenericUnitOfWork<Provider> uow,
            IGenericRepository<Provider> repo) : base(uow)
        {
            _db = db;
            _providerRepository = repo;
        }

        // ---------- GET paginado ----------
        [HttpGet("paged")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<Provider>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<Provider>>> GetPaged(
            [FromQuery] PaginationDTO pagination,
            [FromQuery] bool? isActive = null,
            [FromQuery] DateTime? fromCreated = null,
            [FromQuery] DateTime? toCreated = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(pagination.SortBy)) pagination.SortBy = "Name";
            if (string.IsNullOrWhiteSpace(pagination.SortDir)) pagination.SortDir = "asc";

            IQueryable<Provider> query = _providerRepository.Query();

            if (!string.IsNullOrWhiteSpace(pagination.Term))
            {
                query = query.ApplySearch(
                    pagination.Term,
                    nameof(Provider.Name),
                    nameof(Provider.ContactName),
                    nameof(Provider.TaxId),
                    nameof(Provider.Email),
                    nameof(Provider.Phone));
            }

            if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);
            if (fromCreated.HasValue) query = query.Where(p => p.CreatedAt >= fromCreated.Value);
            if (toCreated.HasValue)
            {
                var inclusive = toCreated.Value.Date.AddDays(1);
                query = query.Where(p => p.CreatedAt < inclusive);
            }

            var ordered = query.ApplySort(pagination.SortBy!, pagination.SortDir!);
            var total = await ordered.CountAsync(ct);
            var items = await ordered.Paginate(pagination).ToListAsync(ct);

            Response.Headers["X-Total-Count"] = total.ToString();

            return Ok(new PagedResult<Provider>
            {
                Items = items,
                Page = pagination.Page,
                PageSize = pagination.RecordsNumber,
                Total = total
            });
        }

        // ---------- GET by id (ruta nombrada para CreatedAtRoute) ----------
        [HttpGet("{id:int}", Name = "Providers_GetById")]
        public new Task<ActionResult<Provider>> GetByIdAsync(int id) => base.GetByIdAsync(id);

        // ---------- CREATE ----------
        [HttpPost]
        [Consumes("application/json")]
        public override async Task<ActionResult<Provider>> PostAsync([FromBody] Provider model)
        {
            // Sanea campos que no deben venir del cliente
            model.Id = 0;
            model.CreatedAt = default;
            // Si tus colecciones son no-nullable puedes omitir asignarlas; EF las ignora al insertar

            Normalize(model);

            // Validación de duplicado de TaxId (incluye inactivos)
            if (!string.IsNullOrWhiteSpace(model.TaxId))
            {
                var exists = await _db.Providers
                    .IgnoreQueryFilters()
                    .AnyAsync(p => p.TaxId == model.TaxId);
                if (exists) return Conflict("Tax Id already exists.");
            }

            var op = await UnitOfWork.AddAsync(model);
            if (!op.WasSuccess) return BadRequest(op.Message);

            // 201 + Location usando la ruta nombrada para evitar 500
            return CreatedAtRoute("Providers_GetById", new { id = op.Result!.Id }, op.Result);
        }

        // ---------- UPDATE ----------
        [HttpPut("{id:int}")]
        public override async Task<IActionResult> PutAsync(int id, [FromBody] Provider model)
        {
            if (id != model.Id) return BadRequest("Id mismatch.");

            var existing = await _db.Providers
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (existing is null) return NotFound();

            Normalize(model);
            model.CreatedAt = existing.CreatedAt; // preservar timestamps de solo lectura

            // Duplicado de TaxId excluyendo el propio registro
            if (!string.IsNullOrWhiteSpace(model.TaxId))
            {
                var duplicate = await _db.Providers
                    .IgnoreQueryFilters()
                    .AnyAsync(p => p.Id != id && p.TaxId == model.TaxId);
                if (duplicate) return Conflict("Tax Id already exists.");
            }

            var op = await UnitOfWork.UpdateAsync(model);
            return op.WasSuccess ? NoContent() : BadRequest(op.Message);
        }

        // ---------- DELETE (con checks de integridad) ----------
        [HttpDelete("{id:int}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            var provider = await _db.Providers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (provider is null) return NotFound();

            bool hasVehicles = await _db.Vehicles.IgnoreQueryFilters().AnyAsync(v => v.ProviderId == id);
            bool hasDrivers = _db.Model.FindEntityType(typeof(Driver)) != null &&
                              await _db.Set<Driver>().IgnoreQueryFilters().AnyAsync(d => d.ProviderId == id);
            bool hasOffers = await _db.VehicleOffers.AnyAsync(o => o.ProviderId == id);
            bool hasCRs = await _db.CapacityRequests.AnyAsync(c => c.ProviderId == id);
            bool hasRoutes = await _db.RoutePlans.AnyAsync(r => r.ProviderId == id);

            if (hasVehicles || hasDrivers || hasOffers || hasCRs || hasRoutes)
                return Conflict("Cannot delete provider because it has related records (vehicles, drivers, offers, capacity requests or routes).");

            var op = await UnitOfWork.DeleteAsync(id);
            return op.WasSuccess ? NoContent() : BadRequest(op.Message);
        }

        // ---------- helpers ----------
        private static void Normalize(Provider p)
        {
            p.Name = (p.Name ?? string.Empty).Trim();
            p.TaxId = string.IsNullOrWhiteSpace(p.TaxId) ? null : p.TaxId.Trim();
            p.ContactName = string.IsNullOrWhiteSpace(p.ContactName) ? null : p.ContactName.Trim();
            p.Email = string.IsNullOrWhiteSpace(p.Email) ? null : p.Email.Trim();
            p.Phone = string.IsNullOrWhiteSpace(p.Phone) ? null : p.Phone.Trim();
            p.Address = string.IsNullOrWhiteSpace(p.Address) ? null : p.Address.Trim();
            // p.IsActive viene del front y se respeta
        }
    }
}