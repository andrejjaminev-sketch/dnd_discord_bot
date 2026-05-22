using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Models;
using DndWorldApi.Services;
using System.Threading.Tasks;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/characters/{charId}/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public InventoryController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<CharacterInventory>> Get(int charId)
        {
            var inv = await _db.GetCharacterInventoryAsync(charId);
            return inv == null ? NotFound() : Ok(inv);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int charId, [FromBody] InventoryChangeDto dto)
        {
            await _db.AddInventoryItemAsync(charId, dto.ItemId, dto.Quantity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> SetQuantity(int charId, [FromBody] InventoryChangeDto dto)
        {
            await _db.SetInventoryItemQuantityAsync(charId, dto.ItemId, dto.Quantity);
            return Ok();
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> Remove(int charId, int itemId)
        {
            await _db.RemoveInventoryItemAsync(charId, itemId);
            return Ok();
        }
    }
}