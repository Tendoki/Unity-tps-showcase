using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class PlayerDbContext : DbContext
{
    public PlayerDbContext(DbContextOptions<PlayerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
}
