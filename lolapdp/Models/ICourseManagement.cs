namespace lolapdp.Models
{
    public interface ICourseManagement
    {
        List<Course> GetAllCourses();
        Course GetCourseById(int id);
        bool AddCourse(Course course);
        bool UpdateCourse(Course course);
        bool DeleteCourse(int id);
    }
}
//dbl