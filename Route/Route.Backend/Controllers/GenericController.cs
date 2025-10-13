using Microsoft.AspNetCore.Mvc;
using Route.Backend.UnitsOfWork.Interfaces;
using Route.Shared.Interfaces;

namespace Route.Backend.Controllers
{
    public abstract class GenericController<T> : ControllerBase where T : class, IEntityWithId
    {
        protected readonly IGenericUnitOfWork<T> UnitOfWork;

        protected GenericController(IGenericUnitOfWork<T> unitOfWork)
            => UnitOfWork = unitOfWork;

        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<T>>> GetAsync()
        {
            var op = await UnitOfWork.GetAsync();
            return op.WasSuccess ? Ok(op.Result) : BadRequest(op.Message);
        }

        [HttpGet("{id:int}")]
        public virtual async Task<ActionResult<T>> GetByIdAsync(int id)
        {
            var op = await UnitOfWork.GetAsync(id);
            return op.WasSuccess && op.Result is not null ? Ok(op.Result) : NotFound();
        }

        [HttpPost]
        public virtual async Task<ActionResult<T>> PostAsync([FromBody] T model)
        {
            var op = await UnitOfWork.AddAsync(model);
            if (!op.WasSuccess) return BadRequest(op.Message);

            // Devuelve Location: /api/{controller}/{id}
            return CreatedAtAction(nameof(GetByIdAsync), new { id = op.Result!.Id }, op.Result);
        }

        [HttpPut("{id:int}")]
        public virtual async Task<IActionResult> PutAsync(int id, [FromBody] T model)
        {
            if (id != model.Id) return BadRequest("Id mismatch.");

            var op = await UnitOfWork.UpdateAsync(model);
            return op.WasSuccess ? NoContent() : BadRequest(op.Message);
        }

        [HttpDelete("{id:int}")]
        public virtual async Task<IActionResult> DeleteAsync(int id)
        {
            var op = await UnitOfWork.DeleteAsync(id);
            return op.WasSuccess ? NoContent() : BadRequest(op.Message);
        }
    }
}