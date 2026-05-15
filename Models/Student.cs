using System.ComponentModel.DataAnnotations;

namespace pro_exam.Models
{
    public class Student
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int Level { get; set; }
        public string Specialization { get; set; }

        public ICollection<StudentCourse> RegisteredCourses { get; set; }
    }
}