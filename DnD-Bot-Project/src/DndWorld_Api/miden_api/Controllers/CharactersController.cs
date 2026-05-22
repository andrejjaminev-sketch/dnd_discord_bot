using Microsoft.AspNetCore.Mvc;
using DndWorldApi.Models;
using DndWorldApi.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DndWorldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharactersController : ControllerBase
    {
        private readonly IDatabaseService _db;
        public CharactersController(IDatabaseService db) => _db = db;

        // ---------- ПРОСМОТР ----------
        [HttpGet("{id}")]
        public async Task<ActionResult<CharFull>> GetById(int id)
        {
            var c = await _db.GetCharFullAsync(id);
            if (c == null) return NotFound();
            return Ok(c);
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<CharFull>> GetByName(string name)
        {
            var c = await _db.GetCharByNameAsync(name);
            if (c == null) return NotFound();
            return Ok(c);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<List<CharFull>>> GetByUserId(int userId)
        {
            var chars = await _db.GetCharsByUserIdAsync(userId);
            if (chars.Count == 0) return NotFound();
            return Ok(chars);
        }

        [HttpGet("by-username/{username}")]
        public async Task<ActionResult<List<CharFull>>> GetByUsername(string username)
        {
            var chars = await _db.GetCharsByUsernameAsync(username);
            if (chars.Count == 0) return NotFound();
            return Ok(chars);
        }

        // ---------- ИЗМЕНЕНИЕ ПОЛЕЙ ----------
        [HttpPatch("{id}/name")]
        public async Task<IActionResult> UpdateName(int id, [FromBody] string name)
        {
            await _db.UpdateCharNameAsync(id, name);
            return Ok();
        }

        [HttpPatch("{id}/age")]
        public async Task<IActionResult> UpdateAge(int id, [FromBody] int age)
        {
            await _db.UpdateCharAgeAsync(id, age);
            return Ok();
        }

        [HttpPatch("{id}/level")]
        public async Task<IActionResult> UpdateLevel(int id, [FromBody] int level)
        {
            await _db.UpdateCharLevelAsync(id, level);
            return Ok();
        }

        [HttpPatch("{id}/user")]
        public async Task<IActionResult> UpdateUserId(int id, [FromBody] int newUserId)
        {
            await _db.UpdateCharUserIdAsync(id, newUserId);
            return Ok();
        }

        [HttpPatch("{id}/species")]
        public async Task<IActionResult> ChangeSpecies(int id, [FromBody] int speciesId)
        {
            await _db.ChangeSpeciesAsync(id, speciesId);
            return Ok();
        }

        // ---------- КЛАССЫ ----------
        [HttpPost("{id}/class")]
        public async Task<IActionResult> AddClass(int id, [FromBody] int classId)
        {
            await _db.AddCharClassAsync(id, classId);
            return Ok();
        }

        [HttpPatch("{id}/class/{classId}/level")]
        public async Task<IActionResult> SetClassLevel(int id, int classId, [FromBody] int lvl)
        {
            await _db.SetCharClassLevelAsync(id, classId, lvl);
            return Ok();
        }

        [HttpDelete("{id}/class/{classId}")]
        public async Task<IActionResult> RemoveClass(int id, int classId)
        {
            await _db.RemoveCharClassAsync(id, classId);
            return Ok();
        }

        [HttpGet("{id}/class/{classId}")]
        public async Task<ActionResult<CharFull.CharClass>> GetClassInfo(int id, int classId)
        {
            var info = await _db.GetCharClassInfoAsync(id, classId);
            if (info == null) return NotFound();
            return Ok(info);
        }

        // ---------- ВЕТКИ ----------
        [HttpPatch("{id}/branch/{branchId}/invest")]
        public async Task<IActionResult> InvestBranchPoints(int id, int branchId, [FromBody] int amount)
        {
            await _db.InvestBranchPointsAsync(id, branchId, amount);
            return Ok();
        }

        [HttpPatch("{id}/branch/{branchId}/reset")]
        public async Task<IActionResult> ResetBranch(int id, int branchId)
        {
            await _db.ResetBranchAsync(id, branchId);
            return Ok();
        }

        [HttpPatch("{id}/branch/{branchId}/level")]
        public async Task<IActionResult> SetBranchLevel(int id, int branchId, [FromBody] int lvl)
        {
            await _db.SetBranchLevelAsync(id, branchId, lvl);
            return Ok();
        }

        [HttpPatch("{id}/branch/{branchId}/points")]
        public async Task<IActionResult> SetBranchPoints(int id, int branchId, [FromBody] int points)
        {
            await _db.SetBranchInvestedPointsAsync(id, branchId, points);
            return Ok();
        }

        // ---------- СПОСОБНОСТИ ----------
        [HttpPost("{id}/ability")]
        public async Task<IActionResult> AddAbility(int id, [FromBody] int abilityId)
        {
            await _db.AddAbilityToCharAsync(id, abilityId);
            return Ok();
        }

        [HttpDelete("{id}/ability/{abilityId}")]
        public async Task<IActionResult> RemoveAbility(int id, int abilityId)
        {
            await _db.RemoveAbilityFromCharAsync(id, abilityId);
            return Ok();
        }

        [HttpPost("{id}/buy-ability")]
        public async Task<IActionResult> BuyAbility(int id, [FromBody] int abilityId)
        {
            await _db.BuyAbilityAsync(id, abilityId);
            return Ok();
        }

        // ---------- ПУЛЫ (новая схема) ----------
        [HttpPost("{id}/pool")]
        public async Task<IActionResult> AddPool(int id, [FromBody] PoolCreateDto dto)
        {
            await _db.AddPoolToCharAsync(id, dto.PoolId, dto.BaseValue, dto.Coefficient);
            return Ok();
        }

        [HttpDelete("{id}/pool/{poolId}")]
        public async Task<IActionResult> RemovePool(int id, int poolId)
        {
            await _db.RemovePoolFromCharAsync(id, poolId);
            return Ok();
        }

        [HttpPatch("{id}/pool/{poolId}/base")]
        public async Task<IActionResult> SetPoolBase(int id, int poolId, [FromBody] int newBase)
        {
            await _db.SetPoolBaseValueAsync(id, poolId, newBase);
            return Ok();
        }

        [HttpPatch("{id}/pool/{poolId}/coefficient")]
        public async Task<IActionResult> SetPoolCoef(int id, int poolId, [FromBody] float coef)
        {
            await _db.SetPoolCoefficientAsync(id, poolId, coef);
            return Ok();
        }

        [HttpPatch("{id}/pool/{poolId}/current")]
        public async Task<IActionResult> SetPoolCurrent(int id, int poolId, [FromBody] int value)
        {
            await _db.SetPoolCurrentAsync(id, poolId, value);
            return Ok();
        }

        // ---------- СТАТЫ ----------
        [HttpPost("{id}/stat")]
        public async Task<IActionResult> AddStat(int id, [FromBody] int statId)
        {
            await _db.AddStatToCharAsync(id, statId);
            return Ok();
        }

        [HttpDelete("{id}/stat/{statId}")]
        public async Task<IActionResult> RemoveStat(int id, int statId)
        {
            await _db.RemoveStatFromCharAsync(id, statId);
            return Ok();
        }

        [HttpPatch("{id}/stat/{statId}/value")]
        public async Task<IActionResult> SetStatValue(int id, int statId, [FromBody] int value)
        {
            await _db.SetStatValueAsync(id, statId, value);
            return Ok();
        }

        // ---------- ЗАМЕТКИ ----------
        [HttpPost("{id}/note")]
        public async Task<IActionResult> AddNote(int id, [FromBody] NoteDto note)
        {
            await _db.AddNoteAsync(id, note.Title, note.Content);
            return Ok();
        }

        [HttpGet("{id}/notes")]
        public async Task<ActionResult<List<CharFull.CharNote>>> GetNotes(int id)
        {
            var notes = await _db.GetCharNotesAsync(id);
            return Ok(notes);
        }

        [HttpGet("{id}/note/{noteId}")]
        public async Task<ActionResult<CharFull.CharNote>> GetNoteById(int id, int noteId)
        {
            var note = await _db.GetNoteByIdAsync(id, noteId);
            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpGet("{id}/note/by-title/{title}")]
        public async Task<ActionResult<CharFull.CharNote>> GetNoteByTitle(int id, string title)
        {
            var note = await _db.GetNoteByTitleAsync(id, title);
            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpDelete("{id}/note/{noteId}")]
        public async Task<IActionResult> RemoveNote(int id, int noteId)
        {
            await _db.RemoveNoteAsync(id, noteId);
            return Ok();
        }

        // ---------- СОЗДАНИЕ / УДАЛЕНИЕ ----------
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] string name)
        {
            var id = await _db.CreateCharacterAsync(name, speciesId: 1, userId: 1);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeleteCharacterAsync(id);
            return Ok();
        }
    }

    // DTO внутри файла, можешь вынести в отдельную папку при желании
    public class PoolCreateDto
    {
        public int PoolId { get; set; }
        public int BaseValue { get; set; } = 10;
        public float Coefficient { get; set; } = 1.0f;
    }

    public class NoteDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}