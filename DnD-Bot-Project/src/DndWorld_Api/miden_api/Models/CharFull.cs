using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace DndWorldApi.Models
{
    public class CharFull
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public int? Age { get; set; }
        public int? FreePoint { get; set; }
        public int SpeciesId { get; set; }
        public string SpeciesName { get; set; }

        public int InventoryId { get; set; }
        
        public CharInventory CharsInventoryInfo { get; set; }

        public List<CharGear> CharsGear { get; set; }

        public List<CharClass> CharsClasses { get; set; }

        public List<CharAbility> NonClassAbilities { get; set; }

        public List<CharPool> CharsPools { get; set; }
        public List<CharEffect> CharsPermanentEffects { get; set; }
        public List<CharEffect> CharsTemporaryEffects { get; set; }

        public List<CharNote> CharNotes { get; set; }

        public List<CharacterStat> Stats { get; set; }


        public class CharInventory
        {
            public int InventoryId { get; set; }
            public decimal MaxWeight { get; set; }

            public List<ItemInventory> Inventory { get; set; }
        }
        public class ItemInventory
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; }
            public string? ItemDescription { get; set; }
            public decimal? ItemWeight { get; set; }
            public int Quantity { get; set; }
        }

        public class CharGear
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; }
            public string? ItemDescription { get; set; }
            public string GearSlot { get; set; }


        }
        public class CharClass
        {
            public int ClassId { get; set; }
            public int Lvl { get; set; }
            public string ClassName { get; set; }
            public string ClassDescription { get; set; }
            public List<CharBranch> Branches { get; set; }
        }

        public class CharBranch
        {
            public int BranchId { get; set; }
            public int InvestedPoints { get; set; }
            public int CurrentLvl { get; set; }
            public string BranchName { get; set; }
            public string BranchDescription { get; set; }
            public List<CharAbility> Abilities { get; set; }

            [JsonIgnore]
            public int ClassId { get; set; }
        }

        public class CharAbility
        {
            public int AbilityId { get; set; }
            public string AbilityName { get; set; }
            public string AbilityDescription { get; set; }
            public int RequiredBranchLvl { get; set; }

            [JsonIgnore]
            public int? BranchId { get; set; }
        }

        public class CharPool
        {
            public int PoolId { get; set; }
            public string PoolName { get; set; }
            public int BaseValue { get; set; }      // бывшее value (базовое значение)
            public float Coefficient { get; set; }  // коэффициент
            public int CurrentValue { get; set; }
            // Вычисляемое максимальное значение
            public int MaxValue => (int)(BaseValue * Coefficient);
        }

        public class CharEffect
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string CategoryName { get; set; }
        }

        public class CharNote
        {
            public int NoteId { get; set; }
            public string NoteTitle { get; set; }
            public string NoteContent { get; set; }
        }
        public class CharacterStat
        {
            public int StatId { get; set; }
            public string StatName { get; set; }
            public int Value { get; set; }
        }




    }
}
