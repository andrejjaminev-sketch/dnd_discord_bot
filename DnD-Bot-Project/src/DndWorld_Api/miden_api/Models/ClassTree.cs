using System.Text.Json.Serialization;

namespace DndWorldApi.Models
{
    public class ClassTree
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string? ClassDescription { get; set; }
        public List<BranchTree> Branches { get; set; }
    }

    public class BranchTree
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchDescription { get; set; }
        public int ClassId { get; set; }
        public List<BranchLevelTree> Levels { get; set; } = new();
    }

    public class AbilityTree
    {
        public int AbilityId { get; set; }
        public string AbilityName { get; set; }
        public string? AbilityDescription { get; set; }
        public int? RequiredLevel { get; set; }

        [JsonIgnore]
        public int BranchId { get; set; }

        // НОВОЕ – стоимость использования (какой пул и сколько)
        public List<AbilityCostInfo>? Costs { get; set; }
    }
    public class BranchLevelTree
    {
        public int Level { get; set; }
        public int RequiredPoints { get; set; }
        public List<AbilityTree> Abilities { get; set; } = new();
    }
}