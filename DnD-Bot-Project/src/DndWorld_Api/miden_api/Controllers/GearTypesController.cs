using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Services;
using System.Threading.Tasks;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GearTypesController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public GearTypesController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.GetAllGearTypesAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            var id = await _db.CreateGearTypeAsync(name);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteGearTypeAsync(id);
            return Ok();
        }
    }
}