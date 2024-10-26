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
    
    public List<Guid> Groups { get; set; } = [];
    public List<Guid> Users { get; set; } = [];
    public List<Guid> Admins { get; set; } = [];
}