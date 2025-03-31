namespace lolapdp.Models
{
    public class StudentCourse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual User Student { get; set; }
        public virtual Course Course { get; set; }
    }
} 