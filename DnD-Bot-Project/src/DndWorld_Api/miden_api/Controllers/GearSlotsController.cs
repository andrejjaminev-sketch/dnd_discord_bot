using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Services;
using System.Threading.Tasks;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GearSlotsController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public GearSlotsController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.GetAllGearSlotsAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            var id = await _db.CreateGearSlotAsync(name);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteGearSlotAsync(id);
            return Ok();
        }
    }
}