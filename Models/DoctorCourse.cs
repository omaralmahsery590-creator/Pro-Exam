namespace pro_exam.Models
{
    public class DoctorCourse
    {
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
