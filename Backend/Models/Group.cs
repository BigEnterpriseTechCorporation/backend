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
    
    public List<Guid> CardsIds { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [ForeignKey("CardsIds")]
    public List<Card> Cards { get; set; }
}