using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class CommentRequest
{
    [MaxLength(500)]
    public string Content { get; set; }
}

public class CommentExport
{
    public string Content { get; set; }
    public string Owner { get; set; }

}

public class Comment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [DataType(DataType.Time)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }
    
    public Guid Card { get; set; }
    
    public Guid Owner { get; set; }
    
    [MaxLength(500)]
    public string Content { get; set; }
}