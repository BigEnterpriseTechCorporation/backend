using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

public class PublicUserDto(User user)
{
    public Guid Id { get; set; } = user.Id;
    public string Name { get; set; } = user.Name;
    public DateTime CreatedAt { get; set; } = user.CreatedAt;
}

public class PrivateUserDto(User user)
{
    public Guid Id { get; set; } = user.Id;
    public string Name { get; set; } = user.Name;
    public string Login { get; set; } = user.Login;
    public List<Guid> Boards { get; set; } = user.Boards;
    public DateTime CreatedAt { get; set; } = user.CreatedAt;
}

public class User
{
    [Key] 
    [System.ComponentModel.DataAnnotations.Schema.Index(IsUnique = true)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [StringLength(50)]
    [Required]
    public string Name { get; set; }
    
    [StringLength(20)]
    [Required]
    //[Key]
    [System.ComponentModel.DataAnnotations.Schema.Index(IsUnique = true)]
    public string Login { get; set; }
    [StringLength(16)]
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [StringLength(32)]
    
    [DataType(DataType.Time)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }
    public byte[]? Avatar { get; set; }

    public List<Guid> Boards { get; set; } = [];
    //public ICollection<BoardUser> BoardUsers { get; set; } = new List<BoardUser>();

    public PublicUserDto PublicDto() => new PublicUserDto(this);
    public PrivateUserDto PrivateDto() => new PrivateUserDto(this);


    public bool VerifyPassword(string password) => HashPassword(password) == Password;

    public static string HashPassword(string password) =>
        Encoding.ASCII.GetString(SHA256.HashData(Encoding.ASCII.GetBytes(password.Insert(4, ")yk!u)cA@79V"))));

}