using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using lolapdp.Models;
using lolapdp.Data;
using Microsoft.EntityFrameworkCore;

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
            return View(courses);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.FacultyList = await _context.Users
                .Where(u => u.Role == "Faculty")
                .Select(u => u.Username)
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
                        .Select(u => u.Username)
                        .ToListAsync();
                    return View(course);
                }

                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.FacultyList = await _context.Users
                .Where(u => u.Role == "Faculty")
                .Select(u => u.Username)
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
                .Select(u => u.Username)
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
                        .Select(u => u.Username)
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
                .Select(u => u.Username)
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

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
//dbl