using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(AppDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult Token([FromBody] LoginPasswordPair pair)
    {
        if (!GetIdentity(pair.Login, pair.Password, out var identity))
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
    public IActionResult Register([FromForm]string login, [FromForm]string password, [FromForm]string username)
    {
        /*(Func<string, bool>, string)[] factorsUsername =
        [
            //(u => db.Users.Any(x => x.Login == u), "Login is already taken"),
            (u => char.IsAscii(u[0]), "Login is alphanumeric"),
            (u => u.Length < 3, $"Login is too short {login.Length < 3}"),
            (u => u.Length > 20, "Login is too long"),
            (u => u[..^1].Any(c => !char.IsLetterOrDigit(c)), "Login is invalid"),
        ];
        foreach (var (f, s) in factorsUsername)
        {
            if (!f(login))
            {
                return BadRequest(new { status = s });
            }
        }

        (Func<string, bool>, string)[] factorsPassword =
        [
            (p => p.Length >= 8, "Minimum Length"),
            (p => p.Any(char.IsUpper), "Uppercase"),
            (p => p.Any(char.IsLower), "Lowercase"),
            (p => p.Any(ch => !char.IsLetterOrDigit(ch)), "Special Characters"),
            (p => p.Any(char.IsDigit), "Digits")
        ];
        foreach (var (f, s) in factorsPassword)
        {
            if (!f(password))
            {
                return BadRequest(new { status = s });
            }
        }*/

        if (username.Length > 50 && db.Users.Any(x => x.Login == login))
        {
            return BadRequest(new { status = "- Username is too long" });
        }
        

        var u = db.Users.Add(new User
        {
            Name = username, 
            Login = login, 
            Password = Models.User.HashPassword(password), 
        });

        db.SaveChanges();

        if (!GetToken(login, password, out var authToken, out var tokenExpires))
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
    [HttpPost("self")]
    public async Task<ActionResult<PrivateKeyStatus>> GetLoggedInUser()
    {
        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound();
        return new ObjectResult(user.PrivateDto());
    }
}