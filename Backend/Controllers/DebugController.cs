using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    AppDbContext db;

    public DebugController(AppDbContext context)
    {
        db = context;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("token")]
    public IActionResult GetToken()
    {
        ClaimsIdentity identity = null;
        var user = db.Users.FirstOrDefault(x => x.Login == "admin");
        if (user != null)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()), //user guid
                new (ClaimsIdentity.DefaultNameClaimType, user.Login), // user login
                //new (ClaimsIdentity.DefaultRoleClaimType, user.Role) // user role
            };
            var claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
            identity = claimsIdentity;
        }
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: TokenOptions.Issuer,
            audience: TokenOptions.Audience,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(TimeSpan.FromMinutes(15)),
            signingCredentials: new SigningCredentials(TokenOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        
        var response = new
        {
            token = encodedJwt,
            id = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
        };
        return Ok(response);
    }

    [HttpGet]
    [Authorize]
    [Route("getlogin")]
    public IActionResult GetLogin()
    {
        return Ok($"Ваш логин: {User.Identity.Name}");
    }

    [HttpGet]
    [Route("boardsusers")]
    public List<Guid> GetBoardUsers(Guid id)
    {
        return db.Boards.Find(id).Users.ToList();
    }

    [HttpGet]
    [Authorize]
    [Route("self")]
    public JsonResult GetSelf()
    {
        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);
        var user =  db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        return new JsonResult(user);
    }
}