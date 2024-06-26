using System.ComponentModel.DataAnnotations;

namespace PlanningEngineerApplication.Models
{
    public class RoomsMeasurementDto
    {
        public int RoomId { get; set; }

        public string RoomName { get; set; }

        public int ApartmentId { get; set; }

        public int ApartmentNumber { get; set; }

        public string BuildingAddress { get; set; }

        [Required(ErrorMessage = "Значение отклонения для потолка обязательно")]
        public double CeilingDeviation { get; set; }

        [Required(ErrorMessage = "Значение отклонения для стены 1 обязательно")]
        public double Wall1Deviation { get; set; }

        [Required(ErrorMessage = "Значение отклонения для стены 2 обязательно")]
        public double Wall2Deviation { get; set; }

        [Required(ErrorMessage = "Значение отклонения для стены 3 обязательно")]
        public double Wall3Deviation { get; set; }

        [Required(ErrorMessage = "Значение отклонения для стены 4 обязательно")]
        public double Wall4Deviation { get; set; }

        [Required(ErrorMessage = "Значение отклонения для пола обязательно")]
        public double FloorDeviation { get; set; }
    }
}
