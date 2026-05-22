using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Models;
using DndWorldApi.Services;
using System.Threading.Tasks;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public ItemsController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.GetAllItemsAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemFull>> GetById(int id)
        {
            var item = await _db.GetItemByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<ItemFull>> GetByName(string name)
        {
            var item = await _db.GetItemByNameAsync(name);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] ItemCreateDto dto)
        {
            var id = await _db.CreateItemAsync(dto.Name, dto.CategoryId, dto.Description,
                dto.Price, dto.Weight, dto.MaxDurability);
            return Ok(id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemUpdateDto dto)
        {
            await _db.UpdateItemAsync(id, dto.Name, dto.Description, dto.Price,
                dto.Weight, dto.MaxDurability, dto.CurrentDurability);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteItemAsync(id);
            return Ok();
        }
    }

    public class ItemCreateDto
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string? Price { get; set; }
        public decimal? Weight { get; set; }
        public int? MaxDurability { get; set; }
    }

    public class ItemUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Price { get; set; }
        public decimal? Weight { get; set; }
        public int? MaxDurability { get; set; }
        public int? CurrentDurability { get; set; }
    }
}