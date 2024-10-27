using VotingApp.Data;
using Microsoft.EntityFrameworkCore;
using WebApplication16.models;
using WebApplication16.Services;

namespace VotingApp.Services;

public class UserService : IUserService
{
    private readonly VotingContext _context;

    public UserService(VotingContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserAdmin>> GetAllUsers()
    {
        var votes=await _context.Votes.ToListAsync();
        var users = new List<UserAdmin>();
        var result= await _context.Users.ToListAsync();
        for (int i = 0; i < result.Count; i++)
        {
            var userAdmin = new UserAdmin
            {
                UserId = result[i].UserId,
                Name = result[i].Name,
                Age = result[i].Age,
                ImageBase64 = "",
                AnnualVoteLimit = result[i].AnnualVoteLimit,
                FaceEmbedding = result[i].FaceEmbedding,
                dot = result[i].dot,
                isUserVotedOnce = votes.FirstOrDefault((v)=>v.myUserId == result[i].UserId)!=null,
            };

            users.Add(userAdmin);
        }
        return users;

    }

    public async Task<User> GetUserById(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> CreateUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUser(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
