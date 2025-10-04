using Route.Shared.DTOs;
using Route.Shared.Responses;

namespace Route.Backend.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<ActionResponse<T>> AddAsync(T entity);

        Task<ActionResponse<T>> UpdateAsync(T entity);

        Task<ActionResponse<T>> DeleteAsync(int id);

        Task<ActionResponse<T>> GetAsync(int id);

        Task<ActionResponse<IEnumerable<T>>> GetAsync();

        Task<ActionResponse<IEnumerable<T>>> GetAsync(PaginationDTO pagination);

        Task<ActionResponse<int>> GetTotalRecordsAsync(PaginationDTO pagination);

        IQueryable<T> Query(bool asNoTracking = true);

        // Listado con filtro + orden
        Task<ActionResponse<IEnumerable<T>>> GetAsync(PaginationDTO pagination, string? term, string? sortBy, string sortDir);

        // Conteo con filtro
        Task<ActionResponse<int>> GetTotalRecordsAsync(string? term);
    }
}