using System;
using System.Collections.Generic;
using System.Linq;

namespace lolapdp.Models
{
    public class UserManagement : IUserManagement
    {
        private List<User> _users;
        private readonly ICSVService _csvService;
        private readonly IAuthentication _authentication;

        public UserManagement(ICSVService csvService, IAuthentication authentication)
        {
            _csvService = csvService;
            _authentication = authentication;
            _users = new List<User>();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                _users = _csvService.ReadCSV<User>("Data/users.csv");
            }
            catch (Exception)
            {
                _users = new List<User>();
            }
        }

        private void SaveUsers()
        {
            try
            {
                _csvService.WriteCSV("Data/users.csv", _users);
            }
            catch (Exception)
            {
                // Log exception
            }
        }

        public List<User> GetAllUsers()
        {
            return _users.ToList();
        }

        public User GetUserById(int id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public User GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public bool AddUser(User user)
        {
            if (_users.Any(u => u.Id == user.Id || u.Username == user.Username))
                return false;

            _users.Add(user);
            SaveUsers();
            return true;
        }

        public bool UpdateUser(User user)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index == -1)
                return false;

            _users[index] = user;
            SaveUsers();
            return true;
        }

        public bool DeleteUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return false;

            _users.Remove(user);
            SaveUsers();
            return true;
        }
    }
} 