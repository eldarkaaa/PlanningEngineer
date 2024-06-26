namespace PlanningEngineerApplication.Models
{
    public class Measurement
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public Rooms Room { get; set; }
        public double CeilingDeviation { get; set; }
        public double Wall1Deviation { get; set; }
        public double Wall2Deviation { get; set; }
        public double Wall3Deviation { get; set; }
        public double Wall4Deviation { get; set; }
        public double FloorDeviation { get; set; }
        public DateTime DateMeasured { get; set; }
    }


}
