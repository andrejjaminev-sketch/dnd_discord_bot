using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Services;
using System.Threading.Tasks;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbilityTypesController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public AbilityTypesController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.GetAllAbilityTypesAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AbilityTypeCreateDto dto)
        {
            var id = await _db.CreateAbilityTypeAsync(dto.Name, dto.Description);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteAbilityTypeAsync(id);
            return Ok();
        }
    }

    public class AbilityTypeCreateDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}