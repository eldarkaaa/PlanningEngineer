using System.ComponentModel.DataAnnotations;

namespace PlanningEngineerApplication.Models
{
    public class ApartmentCreateDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Номер квартиры обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Номер квартиры должен быть числом")]
        public int Number { get; set; }
        public int BuildingId { get; set; }
    }
}
