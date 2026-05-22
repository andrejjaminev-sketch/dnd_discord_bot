using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Models;
using DndWorldApi.Services;
using System.Threading.Tasks;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/characters/{charId}/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public EquipmentController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<CharacterEquipment>> Get(int charId)
        {
            var equip = await _db.GetCharacterEquipmentAsync(charId);
            return equip == null ? NotFound() : Ok(equip);
        }

        [HttpPost]
        public async Task<IActionResult> Equip(int charId, [FromBody] EquipDto dto)
        {
            await _db.EquipItemAsync(charId, dto.ItemId, dto.GearSlotId);
            return Ok();
        }

        [HttpDelete("{slotId}")]
        public async Task<IActionResult> Unequip(int charId, int slotId)
        {
            await _db.UnequipItemAsync(charId, slotId);
            return Ok();
        }
    }
}