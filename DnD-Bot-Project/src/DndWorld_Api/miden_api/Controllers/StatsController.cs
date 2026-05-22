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
    public class StatsController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public StatsController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetAll()
        {
            var list = await _db.GetAllStatsAsync();
            return Ok(list.Select(x => new IdNameDto { Id = x.Id, Name = x.Name }).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdNameDescDto>> GetById(int id)
        {
            var stat = await _db.GetStatByIdAsync(id);
            if (stat == default) return NotFound();
            return Ok(new IdNameDescDto { Id = stat.Id, Name = stat.Name, Description = stat.Description });
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<IdNameDescDto>> GetByName(string name)
        {
            var stat = await _db.GetStatByNameAsync(name);
            if (stat == default) return NotFound();
            return Ok(new IdNameDescDto { Id = stat.Id, Name = stat.Name, Description = stat.Description });
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] StatCreateDto dto)
        {
            var id = await _db.CreateStatAsync(dto.Name, dto.Description);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteStatAsync(id);
            return Ok();
        }
    }

    public class StatCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}