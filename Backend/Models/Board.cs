using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public sealed class BoardUser : User
{
    public string Role { get; set; } = "Guest";
}

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
    public DateTime CreatedAt { get; set; }
    
    public List<Guid> UsersIds { get; set; }
    [ForeignKey("UsersIds")]
    public List<BoardUser> Users { get; set; }
}