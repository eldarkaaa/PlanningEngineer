using System.ComponentModel.DataAnnotations;

namespace PlanningEngineerApplication.Models
{
    public class RoomsDto
    {
        [Required(ErrorMessage = "Название комнаты обязательно")]
        [StringLength(100, ErrorMessage = "Название комнаты не должно превышать 100 символов")]
        public string Name { get; set; }

        public int ApartmentId { get; set; }
    }
}
