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
    public class AbilitiesController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public AbilitiesController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetAll()
        {
            var list = await _db.GetAllAbilitiesShortAsync();
            return Ok(list.Select(x => new IdNameDto { Id = x.Id, Name = x.Name }).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AbilityTree>> GetById(int id)
        {
            var a = await _db.GetAbilityByIdAsync(id);
            return a == null ? NotFound() : Ok(a);
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<AbilityTree>> GetByName(string name)
        {
            var a = await _db.GetAbilityByNameAsync(name);
            return a == null ? NotFound() : Ok(a);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] AbilityCreateDto dto)
        {
            var id = await _db.CreateAbilityAsync(dto.Name, dto.Description, dto.AbilityTypeId);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteAbilityAsync(id);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AbilityUpdateDto dto)
        {
            await _db.UpdateAbilityAsync(id, dto.Name, dto.Description);
            return Ok();
        }

        [HttpPost("{id}/cost")]
        public async Task<IActionResult> SetCost(int id, [FromBody] AbilityCostDto dto)
        {
            await _db.SetAbilityCostAsync(id, dto.PoolId, dto.Cost);
            return Ok();
        }

        [HttpDelete("{id}/cost")]
        public async Task<IActionResult> RemoveCost(int id)
        {
            await _db.RemoveAbilityCostAsync(id);
            return Ok();
        }
    }

    public class AbilityCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int AbilityTypeId { get; set; } = 1; // по умолчанию active
    }

    public class AbilityUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class AbilityCostDto
    {
        public int PoolId { get; set; }
        public int Cost { get; set; }
    }
}