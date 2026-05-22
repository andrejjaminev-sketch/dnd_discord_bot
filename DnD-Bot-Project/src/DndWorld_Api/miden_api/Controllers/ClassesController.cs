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
    public class ClassesController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public ClassesController(IDatabaseService db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetAll()
        {
            var list = await _db.GetAllClassesShortAsync();
            return Ok(list.Select(x => new IdNameDto { Id = x.Id, Name = x.Name }).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClassTree>> GetById(int id)
        {
            var c = await _db.GetClassByIdAsync(id);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<ClassTree>> GetByName(string name)
        {
            var c = await _db.GetClassByNameAsync(name);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] ClassCreateDto dto)
        {
            var id = await _db.CreateClassAsync(dto.Name, dto.Description);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteClassAsync(id);
            return Ok();
        }

        [HttpPatch("{id}/name")]
        public async Task<IActionResult> UpdateName(int id, [FromBody] string name)
        {
            await _db.UpdateClassNameAsync(id, name);
            return Ok();
        }

        [HttpPatch("{id}/description")]
        public async Task<IActionResult> UpdateDescription(int id, [FromBody] string desc)
        {
            await _db.UpdateClassDescriptionAsync(id, desc);
            return Ok();
        }

        [HttpGet("{id}/branches")]
        public async Task<ActionResult<List<IdNameDto>>> GetBranches(int id)
        {
            var branches = await _db.GetBranchesByClassAsync(id);
            return Ok(branches.Select(b => new IdNameDto { Id = b.Id, Name = b.Name }).ToList());
        }
    }

    public class ClassCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}