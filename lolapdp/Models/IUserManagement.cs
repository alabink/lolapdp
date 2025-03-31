namespace lolapdp.Models
{
    public interface IUserManagement
    {
        List<User> GetAllUsers();
        User GetUserById(int id);
        User GetUserByUsername(string username);
        bool AddUser(User user);
        bool UpdateUser(User user);
        bool DeleteUser(int id);
    }
}
//dbl