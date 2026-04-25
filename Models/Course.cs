using System.ComponentModel.DataAnnotations;

namespace pro_exam.Models
{
    public class Course
    {
        public int Id { get; set; }
        [Required]
        public string CourseName { get; set; }
        public int Level { get; set; }
        public string Specialization { get; set; }
        public int NumberOfStudents { get; set; }

        public ICollection<DoctorCourse> DoctorCourses { get; set; }
        public ICollection<StudentCourse> EnrolledStudents { get; set; }
    }
}
