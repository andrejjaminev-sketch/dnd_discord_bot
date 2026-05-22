using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Models;
using DndWorldApi.Services;
using System.Threading.Tasks;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public BranchesController(IDatabaseService db) => _db = db;

        [HttpGet("{id}")]
        public async Task<ActionResult<BranchTree>> GetById(int id)
        {
            var branch = await _db.GetBranchByIdAsync(id);
            return branch == null ? NotFound() : Ok(branch);
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<BranchTree>> GetByName(string name)
        {
            var branch = await _db.GetBranchByNameAsync(name);
            return branch == null ? NotFound() : Ok(branch);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] BranchCreateDto dto)
        {
            var id = await _db.CreateBranchAsync(dto.ClassId, dto.Name, dto.Description);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteBranchAsync(id);
            return Ok();
        }

        [HttpPatch("{id}/name")]
        public async Task<IActionResult> UpdateName(int id, [FromBody] string name)
        {
            await _db.UpdateBranchNameAsync(id, name);
            return Ok();
        }

        [HttpPatch("{id}/description")]
        public async Task<IActionResult> UpdateDescription(int id, [FromBody] string desc)
        {
            await _db.UpdateBranchDescriptionAsync(id, desc);
            return Ok();
        }

        [HttpPost("{branchId}/level")]
        public async Task<IActionResult> CreateLevel(int branchId, [FromBody] BranchLevelCreateDto dto)
        {
            await _db.CreateBranchLevelAsync(branchId, dto.Level, dto.RequiredPoints, dto.AbilityId);
            return Ok();
        }

        [HttpDelete("level/{levelId}")]
        public async Task<IActionResult> DeleteLevel(int levelId)
        {
            await _db.DeleteBranchLevelAsync(levelId);
            return Ok();
        }
    }

    public class BranchCreateDto
    {
        public int ClassId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class BranchLevelCreateDto
    {
        public int Level { get; set; }
        public int RequiredPoints { get; set; }
        public int AbilityId { get; set; }
    }
}