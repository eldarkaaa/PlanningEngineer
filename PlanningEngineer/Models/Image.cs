namespace PlanningEngineerApplication.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int RoomId { get; set; }
        public Rooms Room { get; set; }
    }
}
