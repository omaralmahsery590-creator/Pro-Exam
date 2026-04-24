using System.ComponentModel.DataAnnotations;

namespace pro_exam.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        [Required]
        public string DoctorName { get; set; }

        public ICollection<Montering> Monitorings { get; set; }
        public ICollection<Course> Courses { get; set; }
    }
}
