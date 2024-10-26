using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Card> Cards { get; set; }
    
    public DbSet<Comment> Comments { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { 
        Database.EnsureCreated();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /*modelBuilder.Entity<BoardUser>()
            .HasKey(bu => new { bu.BoardId, bu.UserId }); // Composite key
        modelBuilder.Entity<Board>()
            .HasMany(b => b.BoardUsers)
            .WithOne(bu => bu.Board)
            .HasForeignKey(bu => bu.BoardId);
        modelBuilder.Entity<User>()
            .HasMany(b => b.BoardUsers)
            .WithOne(bu => bu.User)
            .HasForeignKey(bu => bu.UserId);

        modelBuilder.Entity<BoardUser>()
            .HasOne(bu => bu.Board)
            .WithMany(b => b.BoardUsers)
            .HasForeignKey(bu => bu.BoardId);*/

        //modelBuilder.Entity<BoardUser>()
         //   .HasOne(bu => bu.User)
         //   .WithMany(u => u.Boards)
            //.HasForeignKey(bu => bu.UserId);

        // Configure one-to-many relationship between Board and Group
       /* modelBuilder.Entity<Board>()
            .HasMany(b => b.Groups)
            .WithOne(g => g.Board)
            .HasForeignKey(g => g.Board);

        // Configure one-to-many relationship between Group and Card
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Cards)
            .WithOne(c => c.Group)
            .HasForeignKey(c => c.GroupId);
        //modelBuilder.Entity<BoardUser>()
         //   .HasKey(bu => new { bu.BoardId, bu.UserId });*/
    }
}