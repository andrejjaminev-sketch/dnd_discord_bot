namespace DndWorldApi.Models
{
    public class IdNameDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class IdNameDescDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}