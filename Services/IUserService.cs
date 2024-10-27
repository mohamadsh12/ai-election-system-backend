using WebApplication16.models;

namespace WebApplication16.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserAdmin>> GetAllUsers();
        Task<User> GetUserById(int id);
        Task<User> CreateUser(User user);
        Task<User> UpdateUser(User user);
        Task<bool> DeleteUser(int id);

    }
}
