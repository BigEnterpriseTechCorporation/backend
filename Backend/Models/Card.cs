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
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public string Content { get; set; }
}