using Microsoft.AspNetCore.Mvc;
using lolapdp.Models;
using lolapdp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace lolapdp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Authentication _auth;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _auth = new Authentication(_context.Users.ToList());
            CreateDefaultUsers().Wait();
        }

        private async Task CreateDefaultUsers()
        {
            if (!_context.Users.Any())
            {
                var defaultUsers = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        PasswordHash = _auth.HashPassword("admin123"),
                        Role = "Admin"
                    },
                    new User
                    {
                        Username = "student",
                        PasswordHash = _auth.HashPassword("student123"),
                        Role = "Student"
                    },
                    new User
                    {
                        Username = "faculty",
                        PasswordHash = _auth.HashPassword("faculty123"),
                        Role = "Faculty"
                    }
                };

                await _context.Users.AddRangeAsync(defaultUsers);
                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _auth.Login(username, password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Dashboard", "Home");
            }

            ViewData["Error"] = "Invalid username or password";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (_auth.Register(username, password, "Student"))
            {
                var user = new User
                {
                    Username = username,
                    PasswordHash = _auth.HashPassword(password),
                    Role = "Student"
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction(nameof(Login));
            }

            ViewData["Error"] = "Username already exists";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}


//dbl