namespace pro_exam.ViewModel
{
    public class StudentExamItemViewModel
    {
        public string CourseName { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomName { get; set; }
        public List<string> ExtraRoomNames { get; set; } = new();
        public bool HasConflict { get; set; }
    }

    public class StudentExamScheduleViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int Level { get; set; }
        public string Specialization { get; set; }
        public List<StudentExamItemViewModel> Exams { get; set; } = new();
        public bool HasConflict { get; set; }
    }
}
