using Microsoft.EntityFrameworkCore;
using WebApplication16.models;
using VotingApp.Data;

namespace WebApplication16.Services
{
    public class ProtocolService : IProtocolService
    {
        private readonly VotingContext _context;

        public ProtocolService(VotingContext context)
        {
            _context = context;
        }

        public async Task<List<ProtocolEntry>> GetAllAsync()
        {
            return await _context.ProtocolEntries.ToListAsync();
        }

        public async Task AddAsync(ProtocolEntry entry)
        {
            entry.CreatedAt = DateTime.UtcNow;
            await _context.ProtocolEntries.AddAsync(entry);
            await _context.SaveChangesAsync();
        }
    }
}
