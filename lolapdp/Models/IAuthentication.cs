namespace lolapdp.Models
{
    public interface IAuthentication
    {
        bool Register(string username, string password, string role);
        User? Login(string username, string password);
        string? GetUserRole(string username);
        string HashPassword(string password);
    }
}
//dbl