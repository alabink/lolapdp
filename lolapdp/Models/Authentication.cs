using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace lolapdp.Models
{
    public class Authentication : IAuthentication
    {
        private readonly IEnumerable<User> _users;

        public Authentication(IEnumerable<User> users)
        {
            _users = users;
        }

        public bool Register(string username, string password, string role)
        {
            if (_users.Any(u => u.Username == username))
            {
                return false;
            }
            return true;
        }

        public User? Login(string username, string password)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

        public string? GetUserRole(string username)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            return user?.Role;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
        }
    }
}
//dbl