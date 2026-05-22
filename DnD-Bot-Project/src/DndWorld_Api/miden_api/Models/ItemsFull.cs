namespace DndWorldApi.Models
{
    public class ItemsFull
    {
        public List<Item> AllItems { get; set; }

        public class Item
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; }
            public string? ItemDescription { get; set; }
            public decimal? ItemWeight { get; set; }
            public string? ItemPrice { get; set; }
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string? CategoryDescription { get; set; }

            public int? MaxDurability { get; set; }
            public int? CurrentDurability { get; set; }
        }
    }
}