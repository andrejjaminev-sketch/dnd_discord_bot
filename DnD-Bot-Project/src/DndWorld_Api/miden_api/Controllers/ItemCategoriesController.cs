using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Services;
using System.Threading.Tasks;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemCategoriesController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public ItemCategoriesController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.GetAllItemCategoriesAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemCatCreateDto dto)
        {
            var id = await _db.CreateItemCategoryAsync(dto.Name, dto.Description);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteItemCategoryAsync(id);
            return Ok();
        }
    }

    public class ItemCatCreateDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}