using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlanningEngineerApplication.Models
{
    public class Rooms
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя комнаты обязательно")]
        public string Name { get; set; }

        public int ApartmentId { get; set; }
        public Apartment? Apartment { get; set; }
        public string? ImagePath { get; set; } 
        public List<Measurement>? Measurements { get; set; }
        public List<Image>? Images { get; set; } 
    }
}
