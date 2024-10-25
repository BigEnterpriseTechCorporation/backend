using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class PublicUserDto(User user)
{
    public Guid Id { get; set; } = user.Id;
    public string Name { get; set; } = user.Name;
}

public class PrivateUserDto(User user)
{
    public Guid Id { get; set; } = user.Id;
    public string Name { get; set; } = user.Name;
    public string Login { get; set; } = user.Login;
}

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [StringLength(50)]
    [Required]
    public string Name { get; set; }
    
    [StringLength(20)]
    [Required]
    [Index(IsUnique = true)]
    public string Login { get; set; }
    [StringLength(16)]
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [StringLength(32)]
    
    [DataType(DataType.Time)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }
    public byte[]? Avatar { get; set; }

    public PublicUserDto PublicDto() => new PublicUserDto(this);
    public PrivateUserDto PrivateDto() => new PrivateUserDto(this);


    public bool VerifyPassword(string password) => HashPassword(password) == Password;

    public static string HashPassword(string password) =>
        Encoding.ASCII.GetString(SHA256.HashData(Encoding.ASCII.GetBytes(password.Insert(4, ")yk!u)cA@79V"))));

}