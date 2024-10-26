using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Card> Cards { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { 
        Database.EnsureCreated();
    }
}