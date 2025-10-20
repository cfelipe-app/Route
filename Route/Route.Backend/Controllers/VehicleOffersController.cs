using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

        // ----------------------------------------------------------------
        // GET: api/vehicleoffers/{offerId}  (necesario para CreatedAtAction)
        // ----------------------------------------------------------------
        [HttpGet("{offerId:int}", Name = "GetVehicleOfferById")]
        [ProducesResponseType(typeof(VehicleOffer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehicleOffer>> GetById(
            int offerId,
            CancellationToken cancellationToken = default)
        {
            var getResponse = await _vehicleOfferUnitOfWork.GetAsync(offerId);
            if (!getResponse.WasSuccess || getResponse.Result is null)
                return NotFound();

            return Ok(getResponse.Result);
        }

        /// <summary>
        /// Paginado con filtros y orden (devuelve DTO para UI).
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<VehicleOfferDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<VehicleOfferDto>>> GetPaged(
            [FromQuery] PaginationDTO pagination,
            [FromQuery] int? capacityRequestId = null,
            [FromQuery] int? providerId = null,
            [FromQuery] int? vehicleId = null,
            [FromQuery] VehicleOfferStatus? status = null,
            [FromQuery] DateTime? fromCreated = null,
            [FromQuery] DateTime? toCreated = null,
            [FromQuery] bool? visibleForProvider = null,
            CancellationToken cancellationToken = default)
        {
            pagination.SortBy ??= "CreatedAt";
            pagination.SortDir ??= "desc";

            IQueryable<VehicleOffer> query = _vehicleOfferRepository.Query();

            // Búsqueda simple en Notes/Currency
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

            // Visibilidad por CapacityRequest
            if (visibleForProvider == true && providerId.HasValue)
            {
                int pid = providerId.Value;
                query = query.Where(o =>
                    o.CapacityRequest.OnlyTargetProvider == false ||
                    (o.CapacityRequest.OnlyTargetProvider == true &&
                     o.CapacityRequest.ProviderId != null &&
                     o.CapacityRequest.ProviderId == pid));
            }

            var orderedQuery = query.ApplySort(pagination.SortBy!, pagination.SortDir!);

            // 👇 Proyección a DTO (sin Include; EF resuelve Provider.Name)
            var projected = orderedQuery.Select(o => new VehicleOfferDto
            {
                Id = o.Id,
                CapacityRequestId = o.CapacityRequestId,

                ProviderId = o.ProviderId,
                ProviderName = o.Provider.Name,

                Quantity = o.Quantity,
                OfferedWeightKg = o.OfferedWeightKg,
                OfferedVolumeM3 = o.OfferedVolumeM3,

                Price = o.Price,
                Currency = o.Currency,

                PriceMode = o.PriceMode,
                PriceModeText = o.PriceMode.ToString(), // o PriceModeText = o.PriceMode.ToDisplay(),

                Status = o.Status,
                StatusText = o.Status.ToString(),       // o StatusText = o.Status.ToDisplay(),

                CreatedAt = o.CreatedAt,
                ValidUntil = o.ValidUntil,
                Notes = o.Notes
            });

            var totalRecords = await projected.CountAsync(cancellationToken);
            var items = await projected
                .Skip((pagination.Page - 1) * pagination.RecordsNumber)
                .Take(pagination.RecordsNumber)
                .ToListAsync(cancellationToken);

            Response.Headers["X-Total-Count"] = totalRecords.ToString();

            return Ok(new PagedResult<VehicleOfferDto>
            {
                Items = items,
                Page = pagination.Page,
                PageSize = pagination.RecordsNumber,
                Total = totalRecords
            });
        }

        /// <summary>
        /// Ofertas paginadas SOLO del proveedor indicado (atajo para ProviderAdmin).
        /// </summary>
        [HttpGet("by-provider/{providerId:int}")]
        [ProducesResponseType(typeof(PagedResult<VehicleOfferDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<VehicleOfferDto>>> GetByProviderPaged(
            int providerId,
            [FromQuery] PaginationDTO pagination,
            CancellationToken cancellationToken = default)
        {
            pagination.SortBy ??= "CreatedAt";
            pagination.SortDir ??= "desc";

            var query = _vehicleOfferRepository.Query()
                .Where(o => o.ProviderId == providerId);

            var ordered = query.ApplySort(pagination.SortBy!, pagination.SortDir!);

            var projected = ordered.Select(o => new VehicleOfferDto
            {
                Id = o.Id,
                CapacityRequestId = o.CapacityRequestId,

                ProviderId = o.ProviderId,
                ProviderName = o.Provider.Name,

                Quantity = o.Quantity,
                OfferedWeightKg = o.OfferedWeightKg,
                OfferedVolumeM3 = o.OfferedVolumeM3,

                Price = o.Price,
                Currency = o.Currency,

                PriceMode = o.PriceMode,
                PriceModeText = o.PriceMode.ToString(), // o .ToDisplay()

                Status = o.Status,
                StatusText = o.Status.ToString(),       // o .ToDisplay()

                CreatedAt = o.CreatedAt,
                ValidUntil = o.ValidUntil,
                Notes = o.Notes
            });

            var total = await projected.CountAsync(cancellationToken);
            var items = await projected
                .Skip((pagination.Page - 1) * pagination.RecordsNumber)
                .Take(pagination.RecordsNumber)
                .ToListAsync(cancellationToken);

            return Ok(new PagedResult<VehicleOfferDto>
            {
                Items = items,
                Page = pagination.Page,
                PageSize = pagination.RecordsNumber,
                Total = total
            });
        }

        [HttpPut("{offerId:int}/decide")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DecideAsync(
            int offerId,
            [FromBody] DecideOfferDto request,
            CancellationToken cancellationToken = default)
        {
            var getResponse = await _vehicleOfferUnitOfWork.GetAsync(offerId);
            if (!getResponse.WasSuccess || getResponse.Result is null)
                return NotFound();

            var entity = getResponse.Result;

            entity.Status = request.Status;
            entity.DecidedBy = string.IsNullOrWhiteSpace(request.DecidedBy)
                ? (User?.Identity?.Name ?? "system")
                : request.DecidedBy.Trim();
            entity.DecisionAt = DateTime.UtcNow;

            var updateResponse = await _vehicleOfferUnitOfWork.UpdateAsync(entity);
            if (!updateResponse.WasSuccess)
                return Problem(updateResponse.Message ?? "Update failed.", statusCode: StatusCodes.Status409Conflict);

            return NoContent();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(
            [FromBody] SaveVehicleOfferDto request,
            CancellationToken cancellationToken)
        {
            if (request.VehicleId.HasValue && request.VehicleId.Value == 0)
                request.VehicleId = null;

            var currency = string.IsNullOrWhiteSpace(request.Currency)
                ? "PEN"
                : request.Currency.Trim().ToUpperInvariant();
            if (currency.Length > 3) currency = currency[..3];

            var quantity = Math.Max(1, request.Quantity);
            var weight = Math.Max(0, request.OfferedWeightKg);
            var volume = Math.Max(0, request.OfferedVolumeM3);
            var price = Math.Max(0, request.Price);

            if (request.VehicleId.HasValue)
            {
                bool existsSamePlate = await _vehicleOfferRepository.Query()
                    .AnyAsync(o =>
                        o.CapacityRequestId == request.CapacityRequestId &&
                        o.VehicleId == request.VehicleId.Value,
                        cancellationToken);

                if (existsSamePlate)
                    return Conflict("Ya existe una oferta para esta placa en el mismo requerimiento.");
            }

            var entity = new VehicleOffer
            {
                CapacityRequestId = request.CapacityRequestId,
                ProviderId = request.ProviderId,
                VehicleId = request.VehicleId,
                Quantity = quantity,
                OfferedWeightKg = weight,
                OfferedVolumeM3 = volume,
                Price = price,
                Currency = currency,
                PriceMode = request.PriceMode,
                ValidUntil = request.ValidUntil,
                Notes = request.Notes?.Trim(),
                Status = VehicleOfferStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var add = await _vehicleOfferUnitOfWork.AddAsync(entity);
                if (!add.WasSuccess)
                {
                    var msg = add.Message ?? string.Empty;

                    if (msg.Contains("UX_VehicleOffers_ByRequestVehicle", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("IX_VehicleOffers_CapacityRequestId_VehicleId", StringComparison.OrdinalIgnoreCase))
                        return Conflict("Ya existe una oferta para esta placa en el mismo requerimiento.");

                    return Problem(msg == string.Empty ? "Create failed" : msg,
                                   statusCode: StatusCodes.Status409Conflict);
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException se && (se.Number == 2601 || se.Number == 2627))
            {
                var idx = se.Message;

                if (idx.Contains("UX_VehicleOffers_ByRequestVehicle", StringComparison.OrdinalIgnoreCase) ||
                    idx.Contains("IX_VehicleOffers_CapacityRequestId_VehicleId", StringComparison.OrdinalIgnoreCase))
                    return Conflict("Ya existe una oferta para esta placa en el mismo requerimiento.");

                return Conflict(idx);
            }

            return CreatedAtAction(nameof(GetById), new { offerId = entity.Id }, entity);
        }

        //[HttpPut("{offerId:int}")]
        //public async Task<IActionResult> UpdateAsync(
        //    int offerId,
        //    [FromBody] SaveVehicleOfferDto request,
        //    CancellationToken cancellationToken)

        [HttpPut("update/{offerId:int}")]
        public async Task<IActionResult> UpdateAsync(
        int offerId,
        [FromBody] SaveVehicleOfferDto request,
        CancellationToken cancellationToken)
        {
            var getResponse = await _vehicleOfferUnitOfWork.GetAsync(offerId);
            if (!getResponse.WasSuccess || getResponse.Result is null)
                return NotFound();

            var entity = getResponse.Result;

            entity.CapacityRequestId = request.CapacityRequestId;
            entity.ProviderId = request.ProviderId;
            entity.VehicleId = request.VehicleId;
            entity.Quantity = request.Quantity <= 0 ? 1 : request.Quantity;
            entity.OfferedWeightKg = Math.Max(0, request.OfferedWeightKg);
            entity.OfferedVolumeM3 = Math.Max(0, request.OfferedVolumeM3);
            entity.Price = Math.Max(0, request.Price);
            entity.Currency = string.IsNullOrWhiteSpace(request.Currency)
                ? entity.Currency
                : request.Currency.Trim().ToUpperInvariant()[..Math.Min(3, request.Currency.Trim().Length)];
            entity.PriceMode = request.PriceMode;
            entity.ValidUntil = request.ValidUntil;
            entity.Notes = request.Notes?.Trim();

            var updateResponse = await _vehicleOfferUnitOfWork.UpdateAsync(entity);
            if (!updateResponse.WasSuccess)
            {
                if ((updateResponse.Message ?? string.Empty)
                    .Contains("IX_VehicleOffers_CapacityRequestId_VehicleId", StringComparison.OrdinalIgnoreCase))
                    return Conflict("Already exists an offer for this vehicle in the same capacity request.");

                return Problem(updateResponse.Message ?? "Update failed", statusCode: StatusCodes.Status409Conflict);
            }

            return NoContent();
        }

        [HttpGet("lookups/status")]
        [ProducesResponseType(typeof(IEnumerable<EnumLookup<VehicleOfferStatus>>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<EnumLookup<VehicleOfferStatus>>> GetStatusLookups()
        {
            var items = Enum.GetValues<VehicleOfferStatus>()
                .Select(s => new EnumLookup<VehicleOfferStatus>(s, s.ToDisplay()))
                .ToList();

            return Ok(items);
        }
    }
}