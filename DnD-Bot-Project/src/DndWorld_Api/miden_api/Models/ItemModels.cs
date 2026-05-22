using System.Collections.Generic;

namespace DndWorldApi.Models
{
    // Полная информация о предмете
    public class ItemFull
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public string? Price { get; set; }
        public decimal? Weight { get; set; }
        public int? MaxDurability { get; set; }
        public int? CurrentDurability { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        // Вся информация об уроне, защите, использовании теперь в описании
    }

    // Краткая информация о предмете (для списков)
    public class ItemShort
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string? CategoryName { get; set; }
    }

    // Инвентарь персонажа (полный)
    public class CharacterInventory
    {
        public int InventoryId { get; set; }
        public decimal MaxWeight { get; set; }
        public List<InventorySlot> Items { get; set; } = new();
    }

    public class InventorySlot
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal? ItemWeight { get; set; }
    }

    // Экипировка персонажа
    public class CharacterEquipment
    {
        public List<EquippedSlot> Slots { get; set; } = new();
    }

    public class EquippedSlot
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string GearSlot { get; set; }
    }

    // DTO для операций с инвентарём
    public class InventoryChangeDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    // DTO для экипировки
    public class EquipDto
    {
        public int ItemId { get; set; }
        public int GearSlotId { get; set; }
    }
}