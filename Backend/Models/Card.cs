using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class CardExport
{
    public string Name { get; set; }
    public List<string> AssignedUsers { get; set; } = [];
    public List<CommentExport> Comments { get; set; } = [];
    public string Content { get; set; }
}

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
    
    public Guid GroupId { get; set; }
    
    public List<Guid> AssignedUsers { get; set; } = [];
    public List<Guid> Comments { get; set; } = [];
    
    [MaxLength(500)]
    public string Content { get; set; }
}