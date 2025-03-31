using System.Diagnostics;
using lolapdp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using lolapdp.Data;
using Microsoft.EntityFrameworkCore;

namespace lolapdp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Dashboard));
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            if (User.IsInRole("Admin"))
            {
                // Get statistics for admin dashboard
                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.TotalStudents = await _context.Users.CountAsync(u => u.Role == "Student");
                ViewBag.TotalFaculty = await _context.Users.CountAsync(u => u.Role == "Faculty");
                ViewBag.ActiveCourses = await _context.Courses.CountAsync();

                // Get recent user activities
                ViewBag.RecentUserActivities = await _context.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .ToListAsync();

                // Get recent course activities
                ViewBag.RecentCourseActivities = await _context.Courses
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .ToListAsync();
            }
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
//dbl