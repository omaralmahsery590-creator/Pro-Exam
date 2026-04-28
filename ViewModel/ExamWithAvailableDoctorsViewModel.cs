namespace pro_exam.ViewModel
{
    public class ExamWithAvailableDoctorsViewModel
    {
        public int ExamId { get; set; }
        public string CourseName { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomName { get; set; }
        public List<string> ExtraRoomNames { get; set; } = new();
        public List<string> AssignedDoctors { get; set; }
    }
}
