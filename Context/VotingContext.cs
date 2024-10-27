using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication16.models;

namespace VotingApp.Data
{
    public class VotingContext : DbContext
    {
        public VotingContext(DbContextOptions<VotingContext> options)
               : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<ProtocolEntry> ProtocolEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
                .Property(u => u.FaceEmbedding)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions)null));
        }
    }
}
