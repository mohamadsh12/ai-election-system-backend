using WebApplication16.models;

namespace WebApplication16.Services
{
    public interface IVoteService
    {
        Task<Vote> AddVoteAsync(Vote vote);
        Task<bool> DeleteVoteAsync(int id);
        Task<List<Vote>> getAllVotes();
    }
}
