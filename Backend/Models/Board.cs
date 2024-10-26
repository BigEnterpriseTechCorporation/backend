using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class Board
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    [DataType(DataType.Time)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; init; }
/*
    public ICollection<BoardUser> BoardUsers { get; set; } = new List<BoardUser>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();*/
    public User? Creator { get; set; }
    public ICollection<BoardUser> BoardUsers { get; set; }
    public ICollection<Group> Groups { get; set; }
}