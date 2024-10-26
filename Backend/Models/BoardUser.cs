namespace Backend.Models;

public sealed class BoardUser
{
    /*public Guid BoardId { get; set; }
    public Board Board { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public BoardRole Role { get; set; }*/
    public Guid BoardId { get; set; }
    public Board Board { get; set; }
    public string UserId { get; set; }
    public User? User { get; set; }
    public BoardRole Role { get; set; }
}