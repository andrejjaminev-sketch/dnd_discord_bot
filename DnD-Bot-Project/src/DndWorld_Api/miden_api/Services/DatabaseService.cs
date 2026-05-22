using Dapper;
using DndWorldApi.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using static DndWorldApi.Models.CharFull;

namespace DndWorldApi.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<CharFull> GetCharFullAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var charMain = @"
            SELECT 
                c.id, 
                c.full_name AS FullName, 
                c.age, 
                c.free_points AS FreePoint,
                c.species_id AS SpeciesId, 
                s.name AS SpeciesName,
                c.inventory_id AS InventoryId
            FROM dnd.char c
            JOIN dnd.species s ON c.species_id = s.id
            WHERE c.id = @Id";

            var character = await connection.QueryFirstOrDefaultAsync<CharFull>(charMain, new { Id = id });
            if (character == null) return null;

            var charGear = @"
            SELECT
                ei.item_id AS ItemId,
                gs.name AS GearSlot,
                i.name AS ItemName,
                i.description AS ItemDescription
            FROM dnd.equipped_item ei
            JOIN dnd.item i ON ei.item_id = i.id
            JOIN dnd.gear_slot gs ON ei.gear_slot_id = gs.id
            WHERE ei.char_id = @Id";

            character.CharsGear = (await connection.QueryAsync<CharFull.CharGear>(charGear, new { Id = id })).ToList();

            var charClass = @"
            SELECT 
                cc.class_id AS ClassId,
                cc.lvl as Lvl,
                c.name AS ClassName,
                c.description AS ClassDescription
            FROM dnd.char_class cc
            JOIN dnd.class c ON cc.class_id = c.id
            WHERE cc.char_id = @Id";

            var charBranch = @"
            SELECT 
                cb.branch_id AS BranchId,
                cb.invested_points AS InvestedPoints,
                cb.current_lvl AS CurrentLvl,
                b.name AS BranchName,
                b.description AS BranchDescription,
                b.class_id AS ClassId
            FROM dnd.char_branch cb
            JOIN dnd.branch b ON cb.branch_id = b.id
            WHERE cb.char_id = @Id";

            var charAbility = @"
            SELECT 
                ac.ability_id AS AbilityId,
                a.name AS AbilityName,
                a.description AS AbilityDescription,
                ba.branch_id AS BranchId,
                ba.required_branch_lvl AS RequiredBranchLvl   -- добавили
            FROM dnd.ability_char ac
            JOIN dnd.ability a ON ac.ability_id = a.id
            LEFT JOIN dnd.branch_ability ba ON a.id = ba.ability_id
            WHERE ac.char_id = @Id";

            var classes = (await connection.QueryAsync<CharClass>(charClass, new { Id = id })).ToList();
            var branches = (await connection.QueryAsync<CharBranch>(charBranch, new { Id = id })).ToList();
            var abilities = (await connection.QueryAsync<CharAbility>(charAbility, new { Id = id })).ToList();

            var classDict = classes.ToDictionary(c => c.ClassId);

            foreach (var branch in branches)
            {
                if (classDict.TryGetValue(branch.ClassId, out var parentClass))
                {
                    if (parentClass.Branches == null) parentClass.Branches = new List<CharBranch>();
                    parentClass.Branches.Add(branch);
                }
            }

            var branchDict = branches.ToDictionary(b => b.BranchId);
            var NonClassAbilities = new List<CharAbility>();
            foreach (var ability in abilities)
            {
                // Если способность не привязана к ветке (BranchId == null) или её ветка отсутствует у персонажа,
                // то считаем её внеклассовой.
                if (ability.BranchId == null || !branchDict.ContainsKey(ability.BranchId.Value))
                {
                    NonClassAbilities.Add(ability);
                }
                else
                {
                    if (branchDict[ability.BranchId.Value].Abilities == null)
                        branchDict[ability.BranchId.Value].Abilities = new List<CharAbility>();
                    branchDict[ability.BranchId.Value].Abilities.Add(ability);
                }
            }
            character.NonClassAbilities = NonClassAbilities;
            character.CharsClasses = classes;

            var charPools = @"
            SELECT 
                pc.pool_id AS PoolId,
                p.name AS PoolName,
                pc.value AS BaseValue,
                pc.coefficient AS Coefficient,
                pc.current_value AS CurrentValue
            FROM dnd.pool_char pc
            JOIN dnd.pool p ON pc.pool_id = p.id
            WHERE pc.char_id = @Id ";
            character.CharsPools = (await connection.QueryAsync<CharPool>(charPools, new { Id = id })).ToList();

            var charPermanentEffects = @"
            SELECT
                e.id,
                e.name,
                e.description,
                ec.name AS CategoryName
            FROM dnd.effect_char ecf
            JOIN dnd.effect e ON ecf.effect_id = e.id
            JOIN dnd.effect_category ec ON e.effect_category_id = ec.id
            WHERE ecf.char_id = @Id AND ecf.is_temporary = false";
            character.CharsPermanentEffects = (await connection.QueryAsync<CharEffect>(charPermanentEffects, new { Id = id })).ToList();

            var charTemporaryEffects = @"
            SELECT
                e.id,
                e.name,
                e.description,
                ec.name AS CategoryName
            FROM dnd.effect_char ecf
            JOIN dnd.effect e ON ecf.effect_id = e.id
            JOIN dnd.effect_category ec ON e.effect_category_id = ec.id
            WHERE ecf.char_id = @Id AND ecf.is_temporary = true";
            character.CharsTemporaryEffects = (await connection.QueryAsync<CharEffect>(charTemporaryEffects, new { Id = id })).ToList();

            var charNotes = @"
            SELECT 
                n.id AS NoteId,
                n.title AS NoteTitle,
                n.content AS NoteContent
            FROM dnd.char_notes cn
            JOIN dnd.note n ON cn.note_id = n.id
            WHERE cn.char_id = @Id";
            character.CharNotes = (await connection.QueryAsync<CharNote>(charNotes, new { Id = id })).ToList();

            var charInventory = @"
            SELECT 
                id AS InventoryId,
                max_weight AS MaxWeight
            FROM dnd.inventory
            WHERE id = @Id";

            var inventoryInfo = await connection.QueryFirstOrDefaultAsync<CharInventory>(charInventory, new { Id = character.InventoryId });

            if (inventoryInfo != null)
            {
                var charItems = @"
                SELECT 
                    ii.item_id AS ItemId,
                    i.name AS ItemName,
                    i.description AS ItemDescription,
                    i.weight as ItemWeight,
                    ii.quantity
                FROM dnd.inventory_item ii
                JOIN dnd.item i ON ii.item_id = i.id
                WHERE ii.inventory_id = @Id";
                inventoryInfo.Inventory = (await connection.QueryAsync<ItemInventory>(charItems, new { Id = character.InventoryId })).ToList();
                character.CharsInventoryInfo = inventoryInfo;
            }

            var charStats = @"
            SELECT 
                sc.stat_id AS StatId,
                s.name AS StatName,
                sc.value
            FROM dnd.stat_char sc
            JOIN dnd.stat s ON sc.stat_id = s.id
            WHERE sc.char_id = @Id";
            character.Stats = (await connection.QueryAsync<CharacterStat>(charStats, new { Id = id })).ToList();


            return character;
        }


        public async Task<List<ClassTree>> GetFullClassTreeAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var allClasses = @"
            SELECT 
                id AS ClassId, 
                name AS ClassName, 
                description AS ClassDescription
            FROM dnd.class ORDER BY id";

            var allBranches = @"
            SELECT 
                b.id AS BranchId, 
                b.name AS BranchName,   
                b.description AS BranchDescription,
                b.class_id AS ClassId
            FROM dnd.branch b ORDER BY b.id";

            var allBranchLevels = @"
            SELECT 
                bl.branch_id AS BranchId, 
                bl.lvl AS Level, 
                bl.required_points AS RequiredPoints
            FROM dnd.branch_level bl ORDER BY bl.branch_id, bl.lvl";

            var allAbilities = @"
            SELECT 
                a.id AS AbilityId, 
                a.name AS AbilityName, 
                a.description AS AbilityDescription,
                ba.required_branch_lvl AS RequiredLevel, 
                ba.branch_id AS BranchId
            FROM dnd.ability a
            JOIN dnd.branch_ability ba ON a.id = ba.ability_id
            ORDER BY ba.branch_id, ba.required_branch_lvl";

            var classes = (await connection.QueryAsync<ClassTree>(allClasses)).ToList();
            var branches = (await connection.QueryAsync<BranchTree>(allBranches)).ToList();
            var levels = (await connection.QueryAsync<BranchLevelTemp>(allBranchLevels)).ToList(); // временный плоский тип
            var abilities = (await connection.QueryAsync<AbilityTree>(allAbilities)).ToList();

            // Группируем способности по (branchId, level)
            var abilityLookup = abilities
                .GroupBy(a => (a.BranchId, RequiredLevel: a.RequiredLevel ?? 0))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Для каждой ветки строим уровни и кладём в них способности
            foreach (var branch in branches)
            {
                var branchLevels = levels
                    .Where(l => l.BranchId == branch.BranchId)
                    .OrderBy(l => l.Level)
                    .Select(l => new BranchLevelTree
                    {
                        Level = l.Level,
                        RequiredPoints = l.RequiredPoints,
                        Abilities = abilityLookup.TryGetValue((branch.BranchId, l.Level), out var ab)
                            ? ab
                            : new List<AbilityTree>()
                    })
                    .ToList();

                branch.Levels = branchLevels;
            }

            // Группируем ветки по классам
            var classDict = classes.ToDictionary(c => c.ClassId);
            foreach (var branch in branches)
            {
                if (classDict.TryGetValue(branch.ClassId, out var cls))
                {
                    cls.Branches ??= new List<BranchTree>();
                    cls.Branches.Add(branch);
                }
            }

            return classes;
        }

        // Вспомогательный класс для маппинга branch_level
        private class BranchLevelTemp
        {
            public int BranchId { get; set; }
            public int Level { get; set; }
            public int RequiredPoints { get; set; }
        }
        public async Task<ItemsFull> GetItemsFullAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var sql = @"
            SELECT 
                i.id AS ItemId,
                i.name AS ItemName,
                i.description AS ItemDescription,
                i.weight AS ItemWeight,
                i.price AS ItemPrice,
                i.max_durability AS ItemMaxDurability,
                i.current_durability AS ItemCurrentDurability,
                ic.id AS CategoryId,
                ic.name AS CategoryName,
                ic.description AS CategoryDescription
            FROM dnd.item i
            JOIN dnd.item_category ic ON i.item_category_id = ic.id
            ORDER BY i.id";

            var items = (await connection.QueryAsync<ItemsFull.Item>(sql)).ToList();

            return new ItemsFull { AllItems = items };
        }

        // Вставь в DatabaseService.cs

        // ============= ПЕРСОНАЖИ =============

        public async Task<CharFull?> GetCharByNameAsync(string name)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var id = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM dnd.char WHERE full_name ILIKE @Name LIMIT 1", new { Name = name });
            if (id == null) return null;
            return await GetCharFullAsync(id.Value);
        }

        public async Task<List<CharFull>> GetCharsByUserIdAsync(int userId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var ids = await conn.QueryAsync<int>("SELECT id FROM dnd.char WHERE user_id = @UserId", new { UserId = userId });
            var result = new List<CharFull>();
            foreach (var id in ids)
                result.Add(await GetCharFullAsync(id));
            return result;
        }

        public async Task<List<CharFull>> GetCharsByUsernameAsync(string username)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var ids = await conn.QueryAsync<int>(
                "SELECT c.id FROM dnd.char c JOIN dnd.\"user\" u ON c.user_id = u.id WHERE u.username ILIKE @Username",
                new { Username = username });
            var result = new List<CharFull>();
            foreach (var id in ids)
                result.Add(await GetCharFullAsync(id));
            return result;
        }

        public async Task UpdateCharNameAsync(int id, string newName)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.char SET full_name = @Name WHERE id = @Id", new { Id = id, Name = newName });
        }

        public async Task UpdateCharAgeAsync(int id, int newAge)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.char SET age = @Age WHERE id = @Id", new { Id = id, Age = newAge });
        }

        public async Task UpdateCharLevelAsync(int id, int newLevel)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.char SET lvl = @Lvl WHERE id = @Id", new { Id = id, Lvl = newLevel });
        }

        public async Task UpdateCharUserIdAsync(int charId, int newUserId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.char SET user_id = @UserId WHERE id = @Id", new { Id = charId, UserId = newUserId });
        }

        public async Task AddCharClassAsync(int charId, int classId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.char_class (char_id, class_id, lvl) VALUES (@CharId, @ClassId, 1) ON CONFLICT DO NOTHING",
                new { CharId = charId, ClassId = classId });
        }

        public async Task SetCharClassLevelAsync(int charId, int classId, int newLvl)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "UPDATE dnd.char_class SET lvl = @Lvl WHERE char_id = @CharId AND class_id = @ClassId",
                new { CharId = charId, ClassId = classId, Lvl = newLvl });
        }

        public async Task RemoveCharClassAsync(int charId, int classId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.char_class WHERE char_id = @CharId AND class_id = @ClassId",
                new { CharId = charId, ClassId = classId });
        }

        public async Task InvestBranchPointsAsync(int charId, int branchId, int amount)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "UPDATE dnd.char_branch SET invested_points = invested_points + @Amount WHERE char_id = @CharId AND branch_id = @BranchId",
                new { CharId = charId, BranchId = branchId, Amount = amount });
            var maxLvl = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT MAX(lvl) FROM dnd.branch_level WHERE branch_id = @BranchId AND required_points <= (SELECT invested_points FROM dnd.char_branch WHERE char_id = @CharId AND branch_id = @BranchId)",
                new { CharId = charId, BranchId = branchId });
            await conn.ExecuteAsync(
                "UPDATE dnd.char_branch SET current_lvl = @Lvl WHERE char_id = @CharId AND branch_id = @BranchId",
                new { CharId = charId, BranchId = branchId, Lvl = maxLvl });
        }

        public async Task ResetBranchAsync(int charId, int branchId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "UPDATE dnd.char_branch SET invested_points = 0, current_lvl = 0 WHERE char_id = @CharId AND branch_id = @BranchId",
                new { CharId = charId, BranchId = branchId });
        }

        public async Task SetBranchLevelAsync(int charId, int branchId, int newLvl)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "UPDATE dnd.char_branch SET current_lvl = @Lvl WHERE char_id = @CharId AND branch_id = @BranchId",
                new { CharId = charId, BranchId = branchId, Lvl = newLvl });
        }

        public async Task SetBranchInvestedPointsAsync(int charId, int branchId, int points)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "UPDATE dnd.char_branch SET invested_points = @Points WHERE char_id = @CharId AND branch_id = @BranchId",
                new { CharId = charId, BranchId = branchId, Points = points });
        }

        public async Task AddAbilityToCharAsync(int charId, int abilityId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.ability_char (char_id, ability_id) VALUES (@CharId, @AbilityId) ON CONFLICT DO NOTHING",
                new { CharId = charId, AbilityId = abilityId });
        }

        public async Task RemoveAbilityFromCharAsync(int charId, int abilityId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.ability_char WHERE char_id = @CharId AND ability_id = @AbilityId",
                new { CharId = charId, AbilityId = abilityId });
        }

        public async Task AddPoolToCharAsync(int charId, int poolId, int baseValue = 10, float coefficient = 1.0f)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.pool_char (char_id, pool_id, value, coefficient, current_value) VALUES (@CharId, @PoolId, @Value, @Coef, @Value) ON CONFLICT DO NOTHING",
                new { CharId = charId, PoolId = poolId, Value = baseValue, Coef = coefficient });
        }

        public async Task RemovePoolFromCharAsync(int charId, int poolId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.pool_char WHERE char_id = @CharId AND pool_id = @PoolId",
                new { CharId = charId, PoolId = poolId });
        }

        public async Task SetPoolBaseValueAsync(int charId, int poolId, int newValue)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.pool_char SET value = @Value WHERE char_id = @CharId AND pool_id = @PoolId",
                new { CharId = charId, PoolId = poolId, Value = newValue });
        }

        public async Task SetPoolCoefficientAsync(int charId, int poolId, float coeff)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.pool_char SET coefficient = @Coef WHERE char_id = @CharId AND pool_id = @PoolId",
                new { CharId = charId, PoolId = poolId, Coef = coeff });
        }

        public async Task SetPoolCurrentAsync(int charId, int poolId, int newValue)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.pool_char SET current_value = @Value WHERE char_id = @CharId AND pool_id = @PoolId",
                new { CharId = charId, PoolId = poolId, Value = newValue });
        }

        public async Task AddNoteAsync(int charId, string title, string content)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var noteId = await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.note (title, content) VALUES (@Title, @Content) RETURNING id",
                new { Title = title, Content = content });
            await conn.ExecuteAsync("INSERT INTO dnd.char_notes (char_id, note_id) VALUES (@CharId, @NoteId)",
                new { CharId = charId, NoteId = noteId });
        }

        public async Task<List<CharNote>> GetCharNotesAsync(int charId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<CharNote>(
                "SELECT n.id AS NoteId, n.title AS NoteTitle, n.content AS NoteContent FROM dnd.char_notes cn JOIN dnd.note n ON cn.note_id = n.id WHERE cn.char_id = @CharId",
                new { CharId = charId })).ToList();
        }

        public async Task<CharNote?> GetNoteByIdAsync(int charId, int noteId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<CharNote>(
                "SELECT n.id AS NoteId, n.title AS NoteTitle, n.content AS NoteContent FROM dnd.char_notes cn JOIN dnd.note n ON cn.note_id = n.id WHERE cn.char_id = @CharId AND n.id = @NoteId",
                new { CharId = charId, NoteId = noteId });
        }

        public async Task<CharNote?> GetNoteByTitleAsync(int charId, string title)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<CharNote>(
                "SELECT n.id AS NoteId, n.title AS NoteTitle, n.content AS NoteContent FROM dnd.char_notes cn JOIN dnd.note n ON cn.note_id = n.id WHERE cn.char_id = @CharId AND n.title ILIKE @Title",
                new { CharId = charId, Title = title });
        }

        public async Task RemoveNoteAsync(int charId, int noteId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.char_notes WHERE char_id = @CharId AND note_id = @NoteId",
                new { CharId = charId, NoteId = noteId });
            await conn.ExecuteAsync("DELETE FROM dnd.note WHERE id = @NoteId", new { NoteId = noteId });
        }

        public async Task AddStatToCharAsync(int charId, int statId, int value = 1)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.stat_char (char_id, stat_id, value) VALUES (@CharId, @StatId, @Value) ON CONFLICT DO NOTHING",
                new { CharId = charId, StatId = statId, Value = value });
        }

        public async Task RemoveStatFromCharAsync(int charId, int statId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.stat_char WHERE char_id = @CharId AND stat_id = @StatId",
                new { CharId = charId, StatId = statId });
        }

        public async Task SetStatValueAsync(int charId, int statId, int newValue)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.stat_char SET value = @Value WHERE char_id = @CharId AND stat_id = @StatId",
                new { CharId = charId, StatId = statId, Value = newValue });
        }

        public async Task<int> CreateCharacterAsync(string name, int speciesId, int userId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var inventoryId = await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.inventory (max_weight) VALUES (100) RETURNING id");
            var charId = await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.char (user_id, species_id, inventory_id, full_name) VALUES (@UserId, @SpeciesId, @InventoryId, @Name) RETURNING id",
                new { UserId = userId, SpeciesId = speciesId, InventoryId = inventoryId, Name = name });
            return charId;
        }

        public async Task DeleteCharacterAsync(int charId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.char WHERE id = @Id", new { Id = charId });
        }

        public async Task ChangeSpeciesAsync(int charId, int speciesId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.char SET species_id = @SpeciesId WHERE id = @Id",
                new { Id = charId, SpeciesId = speciesId });
        }

        public async Task<CharClass?> GetCharClassInfoAsync(int charId, int classId)
        {
            var full = await GetCharFullAsync(charId);
            return full?.CharsClasses?.FirstOrDefault(c => c.ClassId == classId);
        }

        public async Task BuyAbilityAsync(int charId, int abilityId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var freePoints = await conn.QuerySingleAsync<int>("SELECT free_points FROM dnd.char WHERE id = @Id", new { Id = charId });
            if (freePoints < 1) throw new Exception("Недостаточно свободных очков.");
            await conn.ExecuteAsync("UPDATE dnd.char SET free_points = free_points - 1 WHERE id = @Id", new { Id = charId });
            await conn.ExecuteAsync(
                "INSERT INTO dnd.ability_char (char_id, ability_id) VALUES (@CharId, @AbilityId) ON CONFLICT DO NOTHING",
                new { CharId = charId, AbilityId = abilityId });
        }

        // ============= КЛАССЫ =============

        public async Task<List<(int Id, string Name)>> GetAllClassesShortAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<(int, string)>("SELECT id, name FROM dnd.class ORDER BY id")).ToList();
        }

        public async Task<ClassTree?> GetClassByIdAsync(int id)
        {
            var all = await GetFullClassTreeAsync();
            return all.FirstOrDefault(c => c.ClassId == id);
        }

        public async Task<ClassTree?> GetClassByNameAsync(string name)
        {
            var all = await GetFullClassTreeAsync();
            return all.FirstOrDefault(c => c.ClassName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> CreateClassAsync(string name, string description)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.class (name, description) VALUES (@Name, @Desc) RETURNING id",
                new { Name = name, Desc = description });
        }

        public async Task DeleteClassAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.class WHERE id = @Id", new { Id = id });
        }

        public async Task UpdateClassNameAsync(int id, string name)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.class SET name = @Name WHERE id = @Id", new { Id = id, Name = name });
        }

        public async Task UpdateClassDescriptionAsync(int id, string desc)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.class SET description = @Desc WHERE id = @Id", new { Id = id, Desc = desc });
        }

        public async Task<List<(int Id, string Name)>> GetBranchesByClassAsync(int classId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<(int, string)>(
                "SELECT id, name FROM dnd.branch WHERE class_id = @ClassId ORDER BY id", new { ClassId = classId })).ToList();
        }

        // ============= ВЕТКИ =============

        public async Task<BranchTree?> GetBranchByIdAsync(int id)
        {
            var tree = await GetFullClassTreeAsync();
            return tree.SelectMany(c => c.Branches).FirstOrDefault(b => b.BranchId == id);
        }

        public async Task<BranchTree?> GetBranchByNameAsync(string name)
        {
            var tree = await GetFullClassTreeAsync();
            return tree.SelectMany(c => c.Branches).FirstOrDefault(b => b.BranchName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> CreateBranchAsync(int classId, string name, string description)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.branch (class_id, name, description) VALUES (@ClassId, @Name, @Desc) RETURNING id",
                new { ClassId = classId, Name = name, Desc = description });
        }

        public async Task DeleteBranchAsync(int id)
        {
            await ExecuteSimple("DELETE FROM dnd.branch WHERE id = @Id", new { Id = id });
        }

        public async Task UpdateBranchNameAsync(int id, string name)
        {
            await ExecuteSimple("UPDATE dnd.branch SET name = @Name WHERE id = @Id", new { Id = id, Name = name });
        }

        public async Task UpdateBranchDescriptionAsync(int id, string desc)
        {
            await ExecuteSimple("UPDATE dnd.branch SET description = @Desc WHERE id = @Id", new { Id = id, Desc = desc });
        }

        public async Task CreateBranchLevelAsync(int branchId, int lvl, int requiredPoints, int abilityId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.branch_level (branch_id, lvl, required_points) VALUES (@BranchId, @Lvl, @Points) ON CONFLICT DO NOTHING",
                new { BranchId = branchId, Lvl = lvl, Points = requiredPoints });
            await conn.ExecuteAsync(
                "INSERT INTO dnd.branch_ability (branch_id, ability_id, required_branch_lvl, required_points) VALUES (@BranchId, @AbilityId, @Lvl, 0) ON CONFLICT DO NOTHING",
                new { BranchId = branchId, AbilityId = abilityId, Lvl = lvl });
        }

        public async Task DeleteBranchLevelAsync(int branchLevelId)
        {
            await ExecuteSimple("DELETE FROM dnd.branch_level WHERE id = @Id", new { Id = branchLevelId });
        }

        // ============= СПОСОБНОСТИ =============

        public async Task<List<(int Id, string Name)>> GetAllAbilitiesShortAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<(int, string)>("SELECT id, name FROM dnd.ability ORDER BY id")).ToList();
        }

        public async Task<AbilityTree?> GetAbilityByIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var ability = await conn.QueryFirstOrDefaultAsync<AbilityTree>(
                "SELECT id AS AbilityId, name AS AbilityName, description AS AbilityDescription FROM dnd.ability WHERE id = @Id",
                new { Id = id });

            if (ability != null)
            {
                var costs = await conn.QueryAsync<AbilityCostInfo>(
                    @"SELECT ac.pool_id AS PoolId, p.name AS PoolName, ac.cost AS Cost 
              FROM dnd.ability_cost ac 
              JOIN dnd.pool p ON ac.pool_id = p.id 
              WHERE ac.ability_id = @Id", new { Id = id });
                ability.Costs = costs.ToList();
            }
            return ability;
        }

        public async Task<AbilityTree?> GetAbilityByNameAsync(string name)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var ability = await conn.QueryFirstOrDefaultAsync<AbilityTree>(
                "SELECT id AS AbilityId, name AS AbilityName, description AS AbilityDescription FROM dnd.ability WHERE name ILIKE @Name",
                new { Name = name });

            if (ability != null)
            {
                var costs = await conn.QueryAsync<AbilityCostInfo>(
                    @"SELECT ac.pool_id AS PoolId, p.name AS PoolName, ac.cost AS Cost 
              FROM dnd.ability_cost ac 
              JOIN dnd.pool p ON ac.pool_id = p.id 
              WHERE ac.ability_id = @Id", new { Id = ability.AbilityId });
                ability.Costs = costs.ToList();
            }
            return ability;
        }

        public async Task<int> CreateAbilityAsync(string name, string description, int abilityTypeId = 1)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.ability (name, ability_type_id, description) VALUES (@Name, @TypeId, @Desc) RETURNING id",
                new { Name = name, TypeId = abilityTypeId, Desc = description });
        }

        public async Task DeleteAbilityAsync(int id)
        {
            await ExecuteSimple("DELETE FROM dnd.ability WHERE id = @Id", new { Id = id });
        }

        public async Task UpdateAbilityAsync(int id, string name, string description)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("UPDATE dnd.ability SET name = @Name, description = @Desc WHERE id = @Id",
                new { Id = id, Name = name, Desc = description });
        }

        public async Task SetAbilityCostAsync(int abilityId, int poolId, int cost)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.ability_cost (ability_id, pool_id, cost) VALUES (@AbilityId, @PoolId, @Cost) ON CONFLICT (ability_id, pool_id) DO UPDATE SET cost = @Cost",
                new { AbilityId = abilityId, PoolId = poolId, Cost = cost });
        }

        public async Task RemoveAbilityCostAsync(int abilityId)
        {
            await ExecuteSimple("DELETE FROM dnd.ability_cost WHERE ability_id = @Id", new { Id = abilityId });
        }

        // ============= СТАТЫ =============

        public async Task<List<(int Id, string Name)>> GetAllStatsAsync()
        {
            return (await GetListAsync("dnd.stat")).ToList();
        }

        public async Task<(int Id, string Name, string? Description)> GetStatByIdAsync(int id)
        {
            return await GetSingleAsync("dnd.stat", id);
        }

        public async Task<(int Id, string Name, string? Description)> GetStatByNameAsync(string name)
        {
            return await GetSingleByNameAsync("dnd.stat", name);
        }

        public async Task<int> CreateStatAsync(string name, string description)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.stat (name, description) VALUES (@Name, @Desc) RETURNING id",
                new { Name = name, Desc = description });
        }

        public async Task DeleteStatAsync(int id)
        {
            await ExecuteSimple("DELETE FROM dnd.stat WHERE id = @Id", new { Id = id });
        }

        // ============= ПУЛЫ =============

        public async Task<List<(int Id, string Name)>> GetAllPoolsAsync()
        {
            return (await GetListAsync("dnd.pool")).ToList();
        }

        public async Task<(int Id, string Name, string? Description)> GetPoolByIdAsync(int id)
        {
            return await GetSingleAsync("dnd.pool", id);
        }

        public async Task<(int Id, string Name, string? Description)> GetPoolByNameAsync(string name)
        {
            return await GetSingleByNameAsync("dnd.pool", name);
        }

        public async Task<int> CreatePoolAsync(string name, string description)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.pool (name, description) VALUES (@Name, @Desc) RETURNING id",
                new { Name = name, Desc = description });
        }

        public async Task DeletePoolAsync(int id)
        {
            await ExecuteSimple("DELETE FROM dnd.pool WHERE id = @Id", new { Id = id });
        }

        // ============ ПРЕДМЕТЫ ============

        // Получить все предметы (кратко)
        public async Task<List<ItemShort>> GetAllItemsAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<ItemShort>(
                @"SELECT i.id AS ItemId, i.name AS ItemName, ic.name AS CategoryName
          FROM dnd.item i
          JOIN dnd.item_category ic ON i.item_category_id = ic.id
          ORDER BY i.id")).ToList();
        }

        // Получить предмет по ID (полностью)
        public async Task<ItemFull?> GetItemByIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var sql = @"
        SELECT i.id AS ItemId, i.name AS ItemName, i.description AS ItemDescription,
               i.price, i.weight, i.max_durability AS MaxDurability, i.current_durability AS CurrentDurability,
               ic.id AS CategoryId, ic.name AS CategoryName, ic.description AS CategoryDescription
        FROM dnd.item i
        JOIN dnd.item_category ic ON i.item_category_id = ic.id
        WHERE i.id = @Id";
            return await conn.QueryFirstOrDefaultAsync<ItemFull>(sql, new { Id = id });
        }

        // Поиск по имени
        public async Task<ItemFull?> GetItemByNameAsync(string name)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var id = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM dnd.item WHERE name ILIKE @Name LIMIT 1", new { Name = name });
            if (id == null) return null;
            return await GetItemByIdAsync(id.Value);
        }

        // Создать предмет (базовый)
        public async Task<int> CreateItemAsync(string name, int categoryId, string? description = null,
            string? price = null, decimal? weight = null, int? maxDurability = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            try
            {
                return await conn.QuerySingleAsync<int>(@"
            INSERT INTO dnd.item (item_category_id, name, description, price, weight, max_durability, current_durability)
            VALUES (@CategoryId, @Name, @Description, @Price, @Weight, @MaxDurability, @MaxDurability)
            RETURNING id",
                    new
                    {
                        CategoryId = categoryId,
                        Name = name,
                        Description = description,
                        Price = price,
                        Weight = weight,
                        MaxDurability = maxDurability
                    });
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException($"Предмет с именем '{name}' уже существует.", ex);
            }
        }

        // Обновить основные поля предмета
        public async Task UpdateItemAsync(int id, string? name = null, string? description = null,
            string? price = null, decimal? weight = null, int? maxDurability = null, int? currentDurability = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var updates = new List<string>();
            var param = new DynamicParameters();
            param.Add("Id", id);

            if (name != null) { updates.Add("name = @Name"); param.Add("Name", name); }
            if (description != null) { updates.Add("description = @Description"); param.Add("Description", description); }
            if (price != null) { updates.Add("price = @Price"); param.Add("Price", price); }
            if (weight != null) { updates.Add("weight = @Weight"); param.Add("Weight", weight); }
            if (maxDurability != null) { updates.Add("max_durability = @MaxDur"); param.Add("MaxDur", maxDurability); }
            if (currentDurability != null) { updates.Add("current_durability = @CurDur"); param.Add("CurDur", currentDurability); }

            if (updates.Count > 0)
                await conn.ExecuteAsync($"UPDATE dnd.item SET {string.Join(", ", updates)} WHERE id = @Id", param);
        }

        // Удалить предмет
        public async Task DeleteItemAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.item WHERE id = @Id", new { Id = id });
        }

        // Сделать предмет оружием
        public async Task SetItemAsWeaponAsync(int itemId, int gearTypeId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.weapon (item_id, gear_type_id) VALUES (@ItemId, @GearTypeId) ON CONFLICT DO NOTHING",
                new { ItemId = itemId, GearTypeId = gearTypeId });
        }

        // Сделать предмет бронёй
        public async Task SetItemAsArmorAsync(int itemId, int gearTypeId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.armor (item_id, gear_type_id) VALUES (@ItemId, @GearTypeId) ON CONFLICT DO NOTHING",
                new { ItemId = itemId, GearTypeId = gearTypeId });
        }

        // Сделать предмет расходником
        public async Task SetItemAsConsumableAsync(int itemId, int uses)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO dnd.consumable (item_id, uses) VALUES (@ItemId, @Uses) ON CONFLICT DO NOTHING",
                new { ItemId = itemId, Uses = uses });
        }

        // ============ ИНВЕНТАРЬ ПЕРСОНАЖА ============

        public async Task<CharacterInventory?> GetCharacterInventoryAsync(int charId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var inv = await conn.QueryFirstOrDefaultAsync<CharacterInventory>(
                "SELECT id AS InventoryId, max_weight AS MaxWeight FROM dnd.inventory WHERE id = (SELECT inventory_id FROM dnd.char WHERE id = @CharId)",
                new { CharId = charId });
            if (inv == null) return null;

            var items = await conn.QueryAsync<InventorySlot>(
                @"SELECT ii.item_id AS ItemId, i.name AS ItemName, ii.quantity, i.weight AS ItemWeight
          FROM dnd.inventory_item ii
          JOIN dnd.item i ON ii.item_id = i.id
          WHERE ii.inventory_id = @InvId", new { InvId = inv.InventoryId });
            inv.Items = items.ToList();
            return inv;
        }

        // Добавить/изменить количество предмета в инвентаре
        public async Task AddInventoryItemAsync(int charId, int itemId, int quantity)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var invId = await conn.QuerySingleAsync<int>(
                "SELECT inventory_id FROM dnd.char WHERE id = @Id", new { Id = charId });
            await conn.ExecuteAsync(
                @"INSERT INTO dnd.inventory_item (item_id, inventory_id, quantity) VALUES (@ItemId, @InvId, @Qty)
          ON CONFLICT (item_id, inventory_id) DO UPDATE SET quantity = inventory_item.quantity + @Qty",
                new { ItemId = itemId, InvId = invId, Qty = quantity });
        }

        // Установить точное количество предмета
        public async Task SetInventoryItemQuantityAsync(int charId, int itemId, int quantity)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var invId = await conn.QuerySingleAsync<int>(
                "SELECT inventory_id FROM dnd.char WHERE id = @Id", new { Id = charId });
            if (quantity <= 0)
                await conn.ExecuteAsync("DELETE FROM dnd.inventory_item WHERE item_id = @ItemId AND inventory_id = @InvId",
                    new { ItemId = itemId, InvId = invId });
            else
                await conn.ExecuteAsync(
                    @"INSERT INTO dnd.inventory_item (item_id, inventory_id, quantity) VALUES (@ItemId, @InvId, @Qty)
              ON CONFLICT (item_id, inventory_id) DO UPDATE SET quantity = @Qty",
                    new { ItemId = itemId, InvId = invId, Qty = quantity });
        }

        // Удалить предмет из инвентаря
        public async Task RemoveInventoryItemAsync(int charId, int itemId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var invId = await conn.QuerySingleAsync<int>(
                "SELECT inventory_id FROM dnd.char WHERE id = @Id", new { Id = charId });
            await conn.ExecuteAsync("DELETE FROM dnd.inventory_item WHERE item_id = @ItemId AND inventory_id = @InvId",
                new { ItemId = itemId, InvId = invId });
        }

        // ============ ЭКИПИРОВКА ============

        public async Task<CharacterEquipment?> GetCharacterEquipmentAsync(int charId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var slots = await conn.QueryAsync<EquippedSlot>(
                @"SELECT ei.item_id AS ItemId, i.name AS ItemName, gs.name AS GearSlot
          FROM dnd.equipped_item ei
          JOIN dnd.item i ON ei.item_id = i.id
          JOIN dnd.gear_slot gs ON ei.gear_slot_id = gs.id
          WHERE ei.char_id = @CharId", new { CharId = charId });
            return new CharacterEquipment { Slots = slots.ToList() };
        }

        // Экипировать предмет (заменяет предмет в слоте, если слот занят)
        public async Task EquipItemAsync(int charId, int itemId, int gearSlotId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                @"INSERT INTO dnd.equipped_item (item_id, char_id, gear_slot_id) VALUES (@ItemId, @CharId, @SlotId)
          ON CONFLICT (char_id, gear_slot_id) DO UPDATE SET item_id = @ItemId",
                new { ItemId = itemId, CharId = charId, SlotId = gearSlotId });
        }

        // Снять предмет из слота
        public async Task UnequipItemAsync(int charId, int gearSlotId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "DELETE FROM dnd.equipped_item WHERE char_id = @CharId AND gear_slot_id = @SlotId",
                new { CharId = charId, SlotId = gearSlotId });
        }
        // ============= GEAR TYPE =============
        public async Task<List<IdNameDto>> GetAllGearTypesAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<IdNameDto>("SELECT id, name FROM dnd.gear_type ORDER BY id")).ToList();
        }

        public async Task<int> CreateGearTypeAsync(string name)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.gear_type (name) VALUES (@Name) RETURNING id", new { Name = name });
        }

        public async Task DeleteGearTypeAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.gear_type WHERE id = @Id", new { Id = id });
        }

        // ============= GEAR SLOT =============
        public async Task<List<IdNameDto>> GetAllGearSlotsAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<IdNameDto>("SELECT id, name FROM dnd.gear_slot ORDER BY id")).ToList();
        }

        public async Task<int> CreateGearSlotAsync(string name)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.gear_slot (name) VALUES (@Name) RETURNING id", new { Name = name });
        }

        public async Task DeleteGearSlotAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.gear_slot WHERE id = @Id", new { Id = id });
        }

        // ============= SPECIES =============
        public async Task<List<IdNameDto>> GetAllSpeciesAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<IdNameDto>("SELECT id, name FROM dnd.species ORDER BY id")).ToList();
        }

        public async Task<int> CreateSpeciesAsync(string name, string? description = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.species (name, description) VALUES (@Name, @Desc) RETURNING id",
                new { Name = name, Desc = description });
        }

        public async Task DeleteSpeciesAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.species WHERE id = @Id", new { Id = id });
        }

        // ============= ITEM CATEGORY =============
        public async Task<List<IdNameDto>> GetAllItemCategoriesAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<IdNameDto>("SELECT id, name FROM dnd.item_category ORDER BY id")).ToList();
        }

        public async Task<int> CreateItemCategoryAsync(string name, string? description = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.item_category (name, description) VALUES (@Name, @Desc) RETURNING id",
                new { Name = name, Desc = description });
        }

        public async Task DeleteItemCategoryAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.item_category WHERE id = @Id", new { Id = id });
        }

        // ============= ABILITY TYPE =============
        public async Task<List<IdNameDto>> GetAllAbilityTypesAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return (await conn.QueryAsync<IdNameDto>("SELECT id, name FROM dnd.ability_type ORDER BY id")).ToList();
        }

        public async Task<int> CreateAbilityTypeAsync(string name, string? description = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QuerySingleAsync<int>(
                "INSERT INTO dnd.ability_type (name, description) VALUES (@Name, @Desc) RETURNING id",
                new { Name = name, Desc = description });
        }

        public async Task DeleteAbilityTypeAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM dnd.ability_type WHERE id = @Id", new { Id = id });
        }

        // ---------- вспомогательные методы ----------
        private async Task<IEnumerable<(int Id, string Name)>> GetListAsync(string table)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryAsync<(int, string)>($"SELECT id, name FROM {table} ORDER BY id");
        }

        private async Task<(int Id, string Name, string? Description)> GetSingleAsync(string table, int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<(int, string, string?)>(
                $"SELECT id, name, description FROM {table} WHERE id = @Id", new { Id = id });
        }

        private async Task<(int Id, string Name, string? Description)> GetSingleByNameAsync(string table, string name)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<(int, string, string?)>(
                $"SELECT id, name, description FROM {table} WHERE name ILIKE @Name", new { Name = name });
        }

        private async Task ExecuteSimple(string sql, object param)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, param);
        }
    }
}