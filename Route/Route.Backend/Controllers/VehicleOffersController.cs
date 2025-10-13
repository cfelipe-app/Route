using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Route.Backend.Helpers;
using Route.Backend.Repositories.Interfaces;
using Route.Backend.UnitsOfWork.Interfaces;
using Route.Shared.DTOs;
using Route.Shared.Entities;
using Route.Shared.Enums;
using Route.Shared.Responses;

namespace Route.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleOffersController : GenericController<VehicleOffer>
    {
        private readonly IGenericUnitOfWork<VehicleOffer> _vehicleOfferUnitOfWork;
        private readonly IGenericRepository<VehicleOffer> _vehicleOfferRepository;

        public VehicleOffersController(
            IGenericUnitOfWork<VehicleOffer> vehicleOfferUnitOfWork,
            IGenericRepository<VehicleOffer> vehicleOfferRepository) : base(vehicleOfferUnitOfWork)
        {
            _vehicleOfferUnitOfWork = vehicleOfferUnitOfWork;
            _vehicleOfferRepository = vehicleOfferRepository;
        }

        /// <summary>
        /// Paginado con filtros y orden (sin includes por defecto para ser liviano).
        /// </summary>
        /// <remarks>
        /// Ejemplos:
        /// GET /api/vehicleoffers/paged?page=1&recordsNumber=10&term=urgente
        /// GET /api/vehicleoffers/paged?capacityRequestId=5&status=Accepted&sortBy=CreatedAt&sortDir=desc
        /// GET /api/vehicleoffers/paged?fromCreated=2025-09-01&toCreated=2025-09-30
        /// GET /api/vehicleoffers/paged?visibleForProvider=true&providerId=10 (solo públicas o dirigidas a ese provider)
        /// </remarks>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<VehicleOffer>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<VehicleOffer>>> GetPaged(
            [FromQuery] PaginationDTO pagination,
            [FromQuery] int? capacityRequestId = null,
            [FromQuery] int? providerId = null,
            [FromQuery] int? vehicleId = null,
            [FromQuery] VehicleOfferStatus? status = null,
            [FromQuery] DateTime? fromCreated = null,
            [FromQuery] DateTime? toCreated = null,
            /// <summary>
            /// Si se envía true y además providerId, limita a ofertas cuya CapacityRequest sea pública
            /// (OnlyTargetProvider=false) o privada dirigida al mismo provider (OnlyTargetProvider=true y ProviderId=providerId).
            /// </summary>
            [FromQuery] bool? visibleForProvider = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pagination.SortBy)) pagination.SortBy = "CreatedAt";
            if (string.IsNullOrWhiteSpace(pagination.SortDir)) pagination.SortDir = "desc";

            IQueryable<VehicleOffer> query = _vehicleOfferRepository.Query();

            // Búsqueda simple en Notes/Currency (OR), sin depender de ApplySearch
            if (!string.IsNullOrWhiteSpace(pagination.Term))
            {
                var termLower = pagination.Term.Trim().ToLower();
                query = query.Where(o =>
                    (o.Notes != null && o.Notes.ToLower().Contains(termLower)) ||
                    (o.Currency != null && o.Currency.ToLower().Contains(termLower))
                );
            }

            // Filtros directos
            if (capacityRequestId.HasValue) query = query.Where(o => o.CapacityRequestId == capacityRequestId.Value);
            if (providerId.HasValue) query = query.Where(o => o.ProviderId == providerId.Value);
            if (vehicleId.HasValue) query = query.Where(o => o.VehicleId == vehicleId.Value);
            if (status.HasValue) query = query.Where(o => o.Status == status.Value);

            if (fromCreated.HasValue) query = query.Where(o => o.CreatedAt >= fromCreated.Value);
            if (toCreated.HasValue)
            {
                var inclusive = toCreated.Value.Date.AddDays(1);
                query = query.Where(o => o.CreatedAt < inclusive);
            }

            // Visibilidad basada en la CapacityRequest (útil para ProviderAdmin)
            if (visibleForProvider == true && providerId.HasValue)
            {
                int pid = providerId.Value;
                // Necesitamos chequear la CR relacionada:
                query = query.Where(o =>
                    // CR pública
                    o.CapacityRequest.OnlyTargetProvider == false
                    ||
                    // CR privada dirigida a este proveedor
                    (o.CapacityRequest.OnlyTargetProvider == true &&
                     o.CapacityRequest.ProviderId != null &&
                     o.CapacityRequest.ProviderId == pid)
                );
            }

            var orderedQuery = query.ApplySort(pagination.SortBy, pagination.SortDir);

            var totalRecords = await orderedQuery.CountAsync(cancellationToken);
            var items = await orderedQuery.Paginate(pagination).ToListAsync(cancellationToken);

            Response.Headers["X-Total-Count"] = totalRecords.ToString();

            return Ok(new PagedResult<VehicleOffer>
            {
                Items = items,
                Page = pagination.Page,
                PageSize = pagination.RecordsNumber,
                Total = totalRecords
            });
        }

        /// <summary>
        /// Obtiene ofertas paginadas SOLO del proveedor indicado (atajo común para ProviderAdmin).
        /// </summary>
        [HttpGet("by-provider/{providerId:int}")]
        [ProducesResponseType(typeof(PagedResult<VehicleOffer>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<VehicleOffer>>> GetByProviderPaged(
            int providerId,
            [FromQuery] PaginationDTO pagination,
            CancellationToken cancellationToken = default)
        {
            pagination.SortBy ??= "CreatedAt";
            pagination.SortDir ??= "desc";

            var query = _vehicleOfferRepository.Query()
                .Where(o => o.ProviderId == providerId);

            var ordered = query.ApplySort(pagination.SortBy, pagination.SortDir);

            var total = await ordered.CountAsync(cancellationToken);
            var items = await ordered.Paginate(pagination).ToListAsync(cancellationToken);

            return Ok(new PagedResult<VehicleOffer>
            {
                Items = items,
                Page = pagination.Page,
                PageSize = pagination.RecordsNumber,
                Total = total
            });
        }

        // ----------------------------------------------------------------
        // Acciones de dominio: decidir oferta (Aceptar / Rechazar / Cancelar)
        // ----------------------------------------------------------------

        public class DecideOfferDto
        {
            public VehicleOfferStatus Status { get; set; } // Accepted / Rejected / Cancelled
            public string? DecidedBy { get; set; }
        }

        /// <summary>
        /// Cambia el estado de la oferta y registra DecisionAt/DecidedBy.
        /// </summary>
        [HttpPut("{id:int}/decide")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DecideAsync(
            int id,
            [FromBody] DecideOfferDto body,
            CancellationToken cancellationToken = default)
        {
            var getResponse = await _vehicleOfferUnitOfWork.GetAsync(id);
            if (!getResponse.WasSuccess || getResponse.Result is null)
                return NotFound();

            var entity = getResponse.Result;

            entity.Status = body.Status;
            entity.DecidedBy = body.DecidedBy?.Trim();
            entity.DecisionAt = DateTime.UtcNow;

            var updateResponse = await _vehicleOfferUnitOfWork.UpdateAsync(entity);
            if (!updateResponse.WasSuccess)
                return Problem(updateResponse.Message ?? "Update failed.", statusCode: StatusCodes.Status409Conflict);

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] SaveVehicleOfferDto dto, CancellationToken ct)
        {
            var entity = new VehicleOffer
            {
                CapacityRequestId = dto.CapacityRequestId,
                ProviderId = dto.ProviderId,
                VehicleId = dto.VehicleId,
                OfferedWeightKg = dto.OfferedWeightKg,
                OfferedVolumeM3 = dto.OfferedVolumeM3,
                Price = dto.Price,
                Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "PEN" : dto.Currency.Trim(),
                Notes = dto.Notes?.Trim(),
                Status = VehicleOfferStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            var add = await _vehicleOfferUnitOfWork.AddAsync(entity);
            if (!add.WasSuccess)
            {
                // Manejo de índice único (CapacityRequestId, VehicleId)
                if ((add.Message ?? string.Empty).Contains("IX_VehicleOffers_CapacityRequestId_VehicleId", StringComparison.OrdinalIgnoreCase))
                    return Conflict("Already exists an offer for this vehicle in the same capacity request.");

                return Problem(add.Message ?? "Create failed", statusCode: StatusCodes.Status409Conflict);
            }

            return CreatedAtAction(nameof(GetPaged), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] SaveVehicleOfferDto dto, CancellationToken ct)
        {
            var current = await _vehicleOfferUnitOfWork.GetAsync(id);
            if (!current.WasSuccess || current.Result is null)
                return NotFound();

            var entity = current.Result;

            entity.CapacityRequestId = dto.CapacityRequestId;
            entity.ProviderId = dto.ProviderId;
            entity.VehicleId = dto.VehicleId;
            entity.OfferedWeightKg = dto.OfferedWeightKg;
            entity.OfferedVolumeM3 = dto.OfferedVolumeM3;
            entity.Price = dto.Price;
            entity.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? entity.Currency : dto.Currency.Trim();
            entity.Notes = dto.Notes?.Trim();

            var upd = await _vehicleOfferUnitOfWork.UpdateAsync(entity);
            if (!upd.WasSuccess)
            {
                if ((upd.Message ?? string.Empty).Contains("IX_VehicleOffers_CapacityRequestId_VehicleId", StringComparison.OrdinalIgnoreCase))
                    return Conflict("Already exists an offer for this vehicle in the same capacity request.");

                return Problem(upd.Message ?? "Update failed", statusCode: StatusCodes.Status409Conflict);
            }

            return NoContent();
        }
    }
}