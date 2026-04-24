namespace pro_exam.Models
{
    public class Montering
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; }
    }
}
