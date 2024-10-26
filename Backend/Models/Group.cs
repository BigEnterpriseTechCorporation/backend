using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class Group
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

    /*public Guid BoardId { get; set; }
    public Board Board { get; set; } = null!;

    // Navigation property for cards within the group
    public ICollection<Card> Cards { get; set; } = new List<Card>();*/
    public Board Board { get; set; }
    public ICollection<Card> Cards { get; set; }
}