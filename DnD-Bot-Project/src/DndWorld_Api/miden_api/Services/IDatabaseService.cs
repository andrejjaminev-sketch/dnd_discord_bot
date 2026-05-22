using DndWorldApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static DndWorldApi.Models.CharFull;

namespace DndWorldApi.Services
{
    public interface IDatabaseService
    {
        // ============= ПЕРСОНАЖИ =============
        Task<CharFull?> GetCharFullAsync(int id);
        Task<CharFull?> GetCharByNameAsync(string name);
        Task<List<CharFull>> GetCharsByUserIdAsync(int userId);
        Task<List<CharFull>> GetCharsByUsernameAsync(string username);
        Task UpdateCharNameAsync(int id, string newName);
        Task UpdateCharAgeAsync(int id, int newAge);
        Task UpdateCharLevelAsync(int id, int newLevel);
        Task UpdateCharUserIdAsync(int charId, int newUserId);
        Task AddCharClassAsync(int charId, int classId);
        Task SetCharClassLevelAsync(int charId, int classId, int newLvl);
        Task RemoveCharClassAsync(int charId, int classId);
        Task<CharClass?> GetCharClassInfoAsync(int charId, int classId);
        Task InvestBranchPointsAsync(int charId, int branchId, int amount);
        Task ResetBranchAsync(int charId, int branchId);
        Task SetBranchLevelAsync(int charId, int branchId, int newLvl);
        Task SetBranchInvestedPointsAsync(int charId, int branchId, int points);
        Task AddAbilityToCharAsync(int charId, int abilityId);
        Task RemoveAbilityFromCharAsync(int charId, int abilityId);
        Task BuyAbilityAsync(int charId, int abilityId);
        Task AddPoolToCharAsync(int charId, int poolId, int baseValue, float coefficient);
        Task RemovePoolFromCharAsync(int charId, int poolId);
        Task SetPoolBaseValueAsync(int charId, int poolId, int newBase);
        Task SetPoolCoefficientAsync(int charId, int poolId, float coeff);
        Task SetPoolCurrentAsync(int charId, int poolId, int newValue);
        Task AddStatToCharAsync(int charId, int statId, int value = 1);
        Task RemoveStatFromCharAsync(int charId, int statId);
        Task SetStatValueAsync(int charId, int statId, int newValue);
        Task AddNoteAsync(int charId, string title, string content);
        Task<List<CharNote>> GetCharNotesAsync(int charId);
        Task<CharNote?> GetNoteByIdAsync(int charId, int noteId);
        Task<CharNote?> GetNoteByTitleAsync(int charId, string title);
        Task RemoveNoteAsync(int charId, int noteId);
        Task<int> CreateCharacterAsync(string name, int speciesId, int userId);
        Task DeleteCharacterAsync(int charId);
        Task ChangeSpeciesAsync(int charId, int speciesId);

        // ============= КЛАССЫ =============
        Task<List<(int Id, string Name)>> GetAllClassesShortAsync();
        Task<ClassTree?> GetClassByIdAsync(int id);
        Task<ClassTree?> GetClassByNameAsync(string name);
        Task<int> CreateClassAsync(string name, string description);
        Task DeleteClassAsync(int id);
        Task UpdateClassNameAsync(int id, string name);
        Task UpdateClassDescriptionAsync(int id, string desc);
        Task<List<(int Id, string Name)>> GetBranchesByClassAsync(int classId);

        // ============= ВЕТКИ =============
        Task<BranchTree?> GetBranchByIdAsync(int id);
        Task<BranchTree?> GetBranchByNameAsync(string name);
        Task<int> CreateBranchAsync(int classId, string name, string description);
        Task DeleteBranchAsync(int id);
        Task UpdateBranchNameAsync(int id, string name);
        Task UpdateBranchDescriptionAsync(int id, string desc);
        Task CreateBranchLevelAsync(int branchId, int lvl, int requiredPoints, int abilityId);
        Task DeleteBranchLevelAsync(int branchLevelId);

        // ============= СПОСОБНОСТИ =============
        Task<List<(int Id, string Name)>> GetAllAbilitiesShortAsync();
        Task<AbilityTree?> GetAbilityByIdAsync(int id);
        Task<AbilityTree?> GetAbilityByNameAsync(string name);
        Task<int> CreateAbilityAsync(string name, string description, int abilityTypeId = 1);
        Task DeleteAbilityAsync(int id);
        Task UpdateAbilityAsync(int id, string name, string description);
        Task SetAbilityCostAsync(int abilityId, int poolId, int cost);
        Task RemoveAbilityCostAsync(int abilityId);

        // ============= СТАТЫ =============
        Task<List<(int Id, string Name)>> GetAllStatsAsync();
        Task<(int Id, string Name, string? Description)> GetStatByIdAsync(int id);
        Task<(int Id, string Name, string? Description)> GetStatByNameAsync(string name);
        Task<int> CreateStatAsync(string name, string description);
        Task DeleteStatAsync(int id);

        // ============= ПУЛЫ =============
        Task<List<(int Id, string Name)>> GetAllPoolsAsync();
        Task<(int Id, string Name, string? Description)> GetPoolByIdAsync(int id);
        Task<(int Id, string Name, string? Description)> GetPoolByNameAsync(string name);
        Task<int> CreatePoolAsync(string name, string description);
        Task DeletePoolAsync(int id);

        // ============= ПРЕДМЕТЫ =============
        Task<List<ItemShort>> GetAllItemsAsync();
        Task<ItemFull?> GetItemByIdAsync(int id);
        Task<ItemFull?> GetItemByNameAsync(string name);
        Task<int> CreateItemAsync(string name, int categoryId, string? description = null,
            string? price = null, decimal? weight = null, int? maxDurability = null);
        Task UpdateItemAsync(int id, string? name = null, string? description = null,
            string? price = null, decimal? weight = null, int? maxDurability = null, int? currentDurability = null);
        Task DeleteItemAsync(int id);

        // ============= ИНВЕНТАРЬ =============
        Task<CharacterInventory?> GetCharacterInventoryAsync(int charId);
        Task AddInventoryItemAsync(int charId, int itemId, int quantity);
        Task SetInventoryItemQuantityAsync(int charId, int itemId, int quantity);
        Task RemoveInventoryItemAsync(int charId, int itemId);

        // ============= ЭКИПИРОВКА =============
        Task<CharacterEquipment?> GetCharacterEquipmentAsync(int charId);
        Task EquipItemAsync(int charId, int itemId, int gearSlotId);
        Task UnequipItemAsync(int charId, int gearSlotId);

        // ============= ПОЛНОЕ ДЕРЕВО КЛАССОВ =============
        Task<List<ClassTree>> GetFullClassTreeAsync();
        Task<ItemsFull> GetItemsFullAsync();

        // ============= GEAR TYPE =============
        Task<List<IdNameDto>> GetAllGearTypesAsync();
        Task<int> CreateGearTypeAsync(string name);
        Task DeleteGearTypeAsync(int id);

        // ============= GEAR SLOT =============
        Task<List<IdNameDto>> GetAllGearSlotsAsync();
        Task<int> CreateGearSlotAsync(string name);
        Task DeleteGearSlotAsync(int id);

        // ============= SPECIES =============
        Task<List<IdNameDto>> GetAllSpeciesAsync();
        Task<int> CreateSpeciesAsync(string name, string? description = null);
        Task DeleteSpeciesAsync(int id);

        // ============= ITEM CATEGORY =============
        Task<List<IdNameDto>> GetAllItemCategoriesAsync();
        Task<int> CreateItemCategoryAsync(string name, string? description = null);
        Task DeleteItemCategoryAsync(int id);

        // ============= ABILITY TYPE =============
        Task<List<IdNameDto>> GetAllAbilityTypesAsync();
        Task<int> CreateAbilityTypeAsync(string name, string? description = null);
        Task DeleteAbilityTypeAsync(int id);
    }
}