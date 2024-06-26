using System.ComponentModel.DataAnnotations;

namespace PlanningEngineerApplication.Models
{
    public class Apartment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Номер квартиры обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Номер квартиры должен быть числом")]
        public int Number { get; set; }
        public int BuildingId { get; set; }
        public Building? Building { get; set; }
        public List<Rooms>? Rooms { get; set; }
        public string? ImageFileName { get; set; } 
        public string? ImagePath { get; set; }
    }
}
