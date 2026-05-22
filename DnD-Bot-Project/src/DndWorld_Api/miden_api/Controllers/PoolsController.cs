using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Models;
using DndWorldApi.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoolsController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public PoolsController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetAll()
        {
            var list = await _db.GetAllPoolsAsync();
            return Ok(list.Select(x => new IdNameDto { Id = x.Id, Name = x.Name }).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdNameDescDto>> GetById(int id)
        {
            var pool = await _db.GetPoolByIdAsync(id);
            if (pool == default) return NotFound();
            return Ok(new IdNameDescDto { Id = pool.Id, Name = pool.Name, Description = pool.Description });
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<IdNameDescDto>> GetByName(string name)
        {
            var pool = await _db.GetPoolByNameAsync(name);
            if (pool == default) return NotFound();
            return Ok(new IdNameDescDto { Id = pool.Id, Name = pool.Name, Description = pool.Description });
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] PoolCreateRequestDto dto)
        {
            var id = await _db.CreatePoolAsync(dto.Name, dto.Description);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeletePoolAsync(id);
            return Ok();
        }
    }

    // Переименован, чтобы не конфликтовать с PoolCreateDto из CharactersController
    public class PoolCreateRequestDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}