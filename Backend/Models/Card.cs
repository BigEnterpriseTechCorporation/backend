using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class Card
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
    
    public DateTime EditedAt { get; set; }
    
    public Guid GroupId { get; set; }
    
    public List<Guid> AssignedUsers { get; set; } = [];
    
    [MaxLength(500)]
    public string Content { get; set; }
}