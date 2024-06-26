using PlanningEngineerApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace PlanningEngineerApplication.Models
{
    public class Building
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Адрес обязателен")]
        [StringLength(100, ErrorMessage = "Адрес не должен превышать 100 символов")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Материал обязателен")]
        public BuildingMaterial Material { get; set; }
        [Required(ErrorMessage = "Город обязателен")]
        public string City { get; set; } 
        [Required(ErrorMessage ="Количество квартир обязательно")]
        public int NumberOfApartments { get; set; }

        
        public ICollection<Apartment>? Apartments { get; set; }
    }
}