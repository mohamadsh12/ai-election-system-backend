using WebApplication16.models;

namespace WebApplication16.Services
{
    public interface IProtocolService
    {
        Task<List<ProtocolEntry>> GetAllAsync();
        Task AddAsync(ProtocolEntry entry);
    }
}
