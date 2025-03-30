using System.ComponentModel.DataAnnotations;

namespace lolapdp.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; }

        [Required]
        [StringLength(10)]
        public string CourseCode { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int Credits { get; set; }

        [Required]
        public string Faculty { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
//dbl