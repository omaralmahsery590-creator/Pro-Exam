using System.ComponentModel.DataAnnotations;

namespace pro_exam.Models
{
    public class Room
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int Capacity { get; set; }
    }
}
