namespace pro_exam.Models
{
    public class ExamExtraRoom
    {
        public int Id { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }
    }
}