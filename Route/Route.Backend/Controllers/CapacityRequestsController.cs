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
    public class CapacityRequestsController : GenericController<CapacityRequest>
    {
        private readonly IGenericUnitOfWork<CapacityRequest> _capacityRequestUnitOfWork;
        private readonly IGenericRepository<CapacityRequest> _capacityRequestRepository;

        public CapacityRequestsController(
            IGenericUnitOfWork<CapacityRequest> capacityRequestUnitOfWork,
            IGenericRepository<CapacityRequest> capacityRequestRepository) : base(capacityRequestUnitOfWork)
        {
            _capacityRequestUnitOfWork = capacityRequestUnitOfWork;
            _capacityRequestRepository = capacityRequestRepository;
        }

        /// <summary>
        /// Paginado con filtros: término, estado, rango de fechas, proveedor y visibilidad.
        /// </summary>
        /// Ejemplo:
        /// GET /api/capacityrequests/paged?term=Sur&page=1&recordsNumber=10&sortBy=ServiceDate&sortDir=asc&status=Open&providerId=5&visibleForProvider=true&fromServiceDate=2025-10-01&toServiceDate=2025-10-31
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<CapacityRequest>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CapacityRequest>>> GetPaged(
            [FromQuery] PaginationDTO pagination,
            [FromQuery] CapacityReqStatus? status = null,
            [FromQuery] int? providerId = null,
            /// <summary>
            /// Si es true y se envía providerId, solo devuelve solicitudes visibles para ese proveedor:
            /// públicas (OnlyTargetProvider = false) o privadas dirigidas a ese proveedor (OnlyTargetProvider = true && ProviderId = providerId).
            /// </summary>
            [FromQuery] bool? visibleForProvider = null,
            [FromQuery] DateTime? fromServiceDate = null,
            [FromQuery] DateTime? toServiceDate = null,
            CancellationToken cancellationToken = default)
        {
            var sortBy = string.IsNullOrWhiteSpace(pagination.SortBy) ? "ServiceDate" : pagination.SortBy!;
            var sortDir = string.IsNullOrWhiteSpace(pagination.SortDir) ? "asc" : pagination.SortDir;

            // 1) Base query SIN ordenar
            IQueryable<CapacityRequest> query = _capacityRequestRepository.Query();

            // 2) Filtro por término (OR en Zone / CreatedBy)
            if (!string.IsNullOrWhiteSpace(pagination.Term))
            {
                var termLower = pagination.Term.Trim().ToLower();
                query = query.Where(cr =>
                    (cr.Zone != null && cr.Zone.ToLower().Contains(termLower)) ||
                    (cr.CreatedBy != null && cr.CreatedBy.ToLower().Contains(termLower))
                );
                // Opcional adicional con helper si quieres sumar más campos:
                // query = query.WhereDynamicContains("Zone", pagination.Term);
            }

            // 3) Filtro por estado
            if (status.HasValue)
                query = query.Where(cr => cr.Status == status.Value);

            // 4) Filtro por rango de fechas de servicio
            if (fromServiceDate.HasValue)
                query = query.Where(cr => cr.ServiceDate >= fromServiceDate.Value.Date);

            if (toServiceDate.HasValue)
            {
                var inclusiveEnd = toServiceDate.Value.Date.AddDays(1);
                query = query.Where(cr => cr.ServiceDate < inclusiveEnd);
            }

            // 5) Visibilidad para un proveedor concreto (públicas o privadas dirigidas)
            if (visibleForProvider == true && providerId.HasValue)
            {
                int pid = providerId.Value;
                query = query.Where(cr => !cr.OnlyTargetProvider || (cr.ProviderId != null && cr.ProviderId == pid));
            }

            // (Opcional) Si quisieras filtrar por providerId explícito (no visibilidad sino pertenencia):
            // if (providerId.HasValue) query = query.Where(cr => cr.ProviderId == providerId.Value);

            // 6) Orden SIEMPRE al final
            var orderedQuery = query.ApplySort(sortBy, sortDir);

            // 7) Total + página
            var totalRecords = await orderedQuery.CountAsync(cancellationToken);
            var items = await orderedQuery.Paginate(pagination).ToListAsync(cancellationToken);

            Response.Headers["X-Total-Count"] = totalRecords.ToString();

            return Ok(new PagedResult<CapacityRequest>
            {
                Items = items,
                Page = pagination.Page,
                PageSize = pagination.RecordsNumber,
                Total = totalRecords
            });
        }

        // ---------------------------------------------
        // Acciones de dominio útiles
        // ---------------------------------------------

        /// <summary>
        /// Cambia el estado de una solicitud de capacidad (Open / Closed / Awarded / Cancelled, etc.)
        /// </summary>
        [HttpPut("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateCapacityRequestStatusDto body,
            CancellationToken cancellationToken = default)
        {
            // Usa tu UoW/repo genérico para obtener la entidad
            var getResponse = await _capacityRequestUnitOfWork.GetAsync(id);
            if (!getResponse.WasSuccess || getResponse.Result is null)
                return NotFound();

            var entity = getResponse.Result;

            // Solo actualiza el campo de estado (y quizá notes)
            entity.Status = body.Status;

            // Si llevas auditoría simple, podrías tocar UpdatedAt/UpdatedBy aquí (si existieran)

            var updateResponse = await _capacityRequestUnitOfWork.UpdateAsync(entity);
            if (!updateResponse.WasSuccess)
                return Problem(updateResponse.Message ?? "Update failed.", statusCode: StatusCodes.Status409Conflict);

            return NoContent();
        }

        /// <summary>
        /// Devuelve un resumen rápido (totales por estado en un rango de fecha).
        /// Útil para KPIs del tablero.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(CapacityRequestSummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CapacityRequestSummaryDto>> GetSummary(
            [FromQuery] DateTime? fromServiceDate = null,
            [FromQuery] DateTime? toServiceDate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<CapacityRequest> query = _capacityRequestRepository.Query();

            if (fromServiceDate.HasValue)
                query = query.Where(cr => cr.ServiceDate >= fromServiceDate.Value.Date);

            if (toServiceDate.HasValue)
            {
                var inclusive = toServiceDate.Value.Date.AddDays(1);
                query = query.Where(cr => cr.ServiceDate < inclusive);
            }

            // Agrupa por estado
            var groups = await query
                .GroupBy(cr => cr.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var result = new CapacityRequestSummaryDto
            {
                Total = groups.Sum(x => x.Count),
                ByStatus = groups.ToDictionary(x => x.Status.ToString(), x => x.Count)
            };

            return Ok(result);
        }
    }

    /// <summary>
    /// DTO para actualizar el estado de CapacityRequest.
    /// </summary>
    public class UpdateCapacityRequestStatusDto
    {
        public CapacityReqStatus Status { get; set; }
    }

    /// <summary>
    /// DTO de resumen de CapacityRequests para KPIs.
    /// </summary>
    public class CapacityRequestSummaryDto
    {
        public int Total { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
    }
}