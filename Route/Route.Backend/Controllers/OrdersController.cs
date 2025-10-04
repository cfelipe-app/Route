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
    public class OrdersController : GenericController<Order>
    {
        private readonly IGenericUnitOfWork<Order> _orderUnitOfWork;
        private readonly IGenericRepository<Order> _orderRepository;

        //public OrdersController(IGenericUnitOfWork<Order> unitOfWork) : base(unitOfWork)
        //{
        //}

        public OrdersController(
            IGenericUnitOfWork<Order> orderUnitOfWork,
            IGenericRepository<Order> orderRepository) : base(orderUnitOfWork)
        {
            _orderUnitOfWork = orderUnitOfWork;
            _orderRepository = orderRepository;
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<Order>>> GetPaged([FromQuery] PaginationDTO pagination)
        {
            // Valores predeterminados amigables
            if (string.IsNullOrWhiteSpace(pagination.SortBy))
                pagination.SortBy = "CreatedAt";

            if (string.IsNullOrWhiteSpace(pagination.SortDir))
                pagination.SortDir = "asc";

            var query = _orderRepository.Query()
                                        .ApplyFilter(pagination.Term)
                                        .ApplySort(pagination.SortBy, pagination.SortDir);

            var totalRecords = await query.CountAsync();
            var items = await query.Paginate(pagination).ToListAsync();

            // Cabecera opcional con el total
            Response.Headers["X-Total-Count"] = totalRecords.ToString();

            var result = new PagedResult<Order>
            {
                Items = items,
                Page = pagination.Page,
                PageSize = pagination.RecordsNumber,
                Total = totalRecords
            };

            return Ok(result);
        }
    }
}