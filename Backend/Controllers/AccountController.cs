using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Backend;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Controllers;

public class LoginObject
{
    public string Login { get; set; }
    public string Password { get; set; }
}
public class RegisterObject
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
}

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AccountController(AppDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginObject obj)
    {
        if (!GetIdentity(obj.Login, obj.Password, out var identity))
        {
            return BadRequest(new { status = "Invalid username or password." });
        }

        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: TokenOptions.Issuer,
            audience: TokenOptions.Audience,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(TimeSpan.FromMinutes(TokenOptions.Lifetime)),
            signingCredentials: new SigningCredentials(TokenOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        var response = new
        {
            token = encodedJwt,
            id = identity.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
            expires = (ulong)now.Add(TimeSpan.FromMinutes(TokenOptions.Lifetime)).ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
        };

        return new JsonResult(response);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterObject obj)
    {
        (Func<string, bool>, string)[] factorsUsername =
        [
            (u => db.Users.Any(x => x.Login == u), "Login is already taken"),
            //(u => !char.IsLetter(u.First()), "Login is alphanumeric"),
            (u => u.Length < 3, "Login is too short "),
            (u => u.Length > 20, "Login is too long"),
            (u => u[..^1].Any(c => !char.IsLetter(c)), "Login is invalid"),
        ];
        foreach (var (f, s) in factorsUsername)
        {
            if (f(obj.Login))
            {
                return BadRequest(new { status = s });
            }
        }
        
        if (!new Regex("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,20}$")
                .Match(obj.Password).Success
            )
        {
            return BadRequest(new {status = "Password is not valid."});
        }
        
        if (obj.Name.Length > 50)
        {
            return BadRequest(new { status = "Username is too long" });
        }
        

        var u = db.Users.Add(new User
        {
            Name = obj.Name, 
            Login = obj.Login, 
            Password = Models.User.HashPassword(obj.Password), 
        });

        db.SaveChanges();

        if (!GetToken(obj.Login, obj.Password, out var authToken, out var tokenExpires))
        {
            return BadRequest("Iternal server error");
        }
        
        var resp = new
        {
            status = "success",
            id = u.Entity.Id,
            token = authToken,
            expires = tokenExpires,
        };
        return new JsonResult(resp);
    }

    private bool GetToken(string login, string password, out string token, out ulong expires)
    {
        token = string.Empty;
        expires = 0;
        if (!GetIdentity( login,  password, out var identity))
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: TokenOptions.Issuer,
            audience: TokenOptions.Audience,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(TimeSpan.FromMinutes(TokenOptions.Lifetime)),
            signingCredentials: new SigningCredentials(TokenOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));
        
        token = new JwtSecurityTokenHandler().WriteToken(jwt);

        expires = (ulong)now.Add(TimeSpan.FromMinutes(TokenOptions.Lifetime)).ToUniversalTime()
            .Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        
        return true;
    }
    private bool GetIdentity(string username, string password, out ClaimsIdentity identity)
    {
        var user = db.Users.FirstOrDefault(x => x.Login == username);
        identity = null;
        if (user == null) return false;
        if (!user.VerifyPassword(password)) return false;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), //user guid
            new(ClaimsIdentity.DefaultNameClaimType, user.Login), // user login
            //new(ClaimsIdentity.DefaultRoleClaimType, user.Role) // user role
        };
        var claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
        identity = claimsIdentity;
        return true;
    }

    [Authorize]
    [HttpGet("self")]
    public async Task<ActionResult<PrivateUserDto>> GetLoggedInUser()
    {
        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);
        var user = await db.Users.FindAsync(userId);
        if (user == null)
            return BadRequest();
        return new ObjectResult(user.PrivateDto());
    }
}