using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using lolapdp.Models;
using lolapdp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace lolapdp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses.ToListAsync();
            ViewBag.FacultyList = await _context.Users
                .Where(u => u.Role == "Faculty")
                .ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.FacultyList = await _context.Users
                .Where(u => u.Role == "Faculty")
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Courses.AnyAsync(c => c.CourseCode == course.CourseCode))
                {
                    ModelState.AddModelError("CourseCode", "Course code already exists");
                    ViewBag.FacultyList = await _context.Users
                        .Where(u => u.Role == "Faculty")
                        .ToListAsync();
                    return View(course);
                }

                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.FacultyList = await _context.Users
                .Where(u => u.Role == "Faculty")
                .ToListAsync();
            return View(course);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.FacultyList = await _context.Users
                .Where(u => u.Role == "Faculty")
                .ToListAsync();

            // Use a direct approach instead of SQL queries
            var rawSql = "SELECT DISTINCT StudentId FROM StudentCourses WHERE CourseId = {0}";
            var enrolledStudentIds = new List<int>();
            
            try {
                // Try to manually execute the SQL and get the results
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = string.Format(rawSql, id.Value);
                    
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();
                        
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            enrolledStudentIds.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex) {
                // Log the exception if needed
                // Just continue with empty list if there's an error
            }

            ViewBag.EnrolledStudents = await _context.Users
                .Where(u => u.Role == "Student" && enrolledStudentIds.Contains(u.Id))
                .ToListAsync();

            // Load available students (not enrolled in this course)
            ViewBag.AvailableStudents = await _context.Users
                .Where(u => u.Role == "Student" && !enrolledStudentIds.Contains(u.Id))
                .ToListAsync();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingCourse = await _context.Courses
                    .Where(c => c.CourseCode == course.CourseCode && c.Id != course.Id)
                    .FirstOrDefaultAsync();

                if (existingCourse != null)
                {
                    ModelState.AddModelError("CourseCode", "Course code already exists");
                    ViewBag.FacultyList = await _context.Users
                        .Where(u => u.Role == "Faculty")
                        .ToListAsync();
                    return View(course);
                }

                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.FacultyList = await _context.Users
                .Where(u => u.Role == "Faculty")
                .ToListAsync();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(int courseId, int studentId)
        {
            // Check if course and student exist
            var course = await _context.Courses.FindAsync(courseId);
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Student");
            
            if (course == null || student == null)
            {
                return NotFound();
            }

            try
            {
                // Check if student is already enrolled
                bool isEnrolled = false;
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(1) FROM StudentCourses WHERE CourseId = @courseId AND StudentId = @studentId";
                    
                    var courseIdParam = command.CreateParameter();
                    courseIdParam.ParameterName = "@courseId";
                    courseIdParam.Value = courseId;
                    command.Parameters.Add(courseIdParam);
                    
                    var studentIdParam = command.CreateParameter();
                    studentIdParam.ParameterName = "@studentId";
                    studentIdParam.Value = studentId;
                    command.Parameters.Add(studentIdParam);
                    
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();
                        
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    isEnrolled = count > 0;
                }
                
                if (isEnrolled)
                {
                    // Student already enrolled
                    return RedirectToAction(nameof(Edit), new { id = courseId });
                }

                // Insert using ADO.NET
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "INSERT INTO StudentCourses (StudentId, CourseId, EnrollmentDate) VALUES (@studentId, @courseId, @date)";
                    
                    var courseIdParam = command.CreateParameter();
                    courseIdParam.ParameterName = "@courseId";
                    courseIdParam.Value = courseId;
                    command.Parameters.Add(courseIdParam);
                    
                    var studentIdParam = command.CreateParameter();
                    studentIdParam.ParameterName = "@studentId";
                    studentIdParam.Value = studentId;
                    command.Parameters.Add(studentIdParam);
                    
                    var dateParam = command.CreateParameter();
                    dateParam.ParameterName = "@date";
                    dateParam.Value = DateTime.Now;
                    command.Parameters.Add(dateParam);
                    
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();
                        
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception and return to the edit page
                return RedirectToAction(nameof(Edit), new { id = courseId });
            }

            return RedirectToAction(nameof(Edit), new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(int courseId, int studentId)
        {
            try
            {
                // Delete using ADO.NET
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "DELETE FROM StudentCourses WHERE CourseId = @courseId AND StudentId = @studentId";
                    
                    var courseIdParam = command.CreateParameter();
                    courseIdParam.ParameterName = "@courseId";
                    courseIdParam.Value = courseId;
                    command.Parameters.Add(courseIdParam);
                    
                    var studentIdParam = command.CreateParameter();
                    studentIdParam.ParameterName = "@studentId";
                    studentIdParam.Value = studentId;
                    command.Parameters.Add(studentIdParam);
                    
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();
                        
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
            }

            return RedirectToAction(nameof(Edit), new { id = courseId });
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
//dbl