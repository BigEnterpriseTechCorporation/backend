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
    public string Email { get; set; } = user.Email;
    public string Description { get; set; } = user.Description;
}

public class LoginPasswordPair
{
    public string Login { get; set; }
    public string Password { get; set; }
}
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [StringLength(32)]
    [Required]
    public string Name { get; set; }
    
    [StringLength(32)]
    public string Login { get; set; }
    [StringLength(16)]
    public string Role { get; set; } = "Member";
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [StringLength(32)]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    [StringLength(1024)]
    public string Description { get; set; } = "";
    
    [DataType(DataType.Time)]
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public DateTime Birth { get; set; }
    public uint Rating { get; set; }
    /*
     * награды
     */

    public byte[]? Avatar { get; set; }

    public PublicUserDto PublicDto() => new PublicUserDto(this);
    public PrivateUserDto PrivateDto() => new PrivateUserDto(this);


    public bool VerifyPassword(string password)
    {
        var hash = Encoding.ASCII.GetString(SHA256.HashData(Encoding.ASCII.GetBytes(password.Insert(4, ")yk!u)cA@79V"))));
        return hash == Password;
    }

}