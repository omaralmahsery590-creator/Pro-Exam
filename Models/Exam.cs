using System.ComponentModel.DataAnnotations;

namespace pro_exam.Models
{
    public class Exam
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public ICollection<Montering> Monitorings { get; set; }
        public ICollection<ExamExtraRoom> ExtraRooms { get; set; }
    }
}
