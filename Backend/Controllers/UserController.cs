using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using ObjectResult = Microsoft.AspNetCore.Mvc.ObjectResult;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    AppDbContext db;
    public UsersController(AppDbContext context)
    {
        db = context;
        if (db.Users.Any()) return;
        db.Users.Add(new User { Name = "Big Lebowski", Login = "admin", Password = Models.User.HashPassword("admin")});
        db.SaveChanges();
    }
    
    //GET User list
    [AllowAnonymous]
    [HttpGet]
    public async Task<List<PublicUserDto>> Get()
    {
        return await db.Users.Select(u => u.PublicDto()).ToListAsync();
    }

    //GET Users by id
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicUserDto>> Get(Guid id)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return NotFound();
        return new ObjectResult(user.PublicDto());
    }
    
    //GET Avatar by user id
    [AllowAnonymous]
    [HttpGet("{id:guid}/avatar")]
    public async Task<ActionResult> GetAvatar(Guid id)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return NotFound();
        if (user.Avatar != null) return new FileContentResult(user.Avatar, "image/webp");
        return NotFound();
    }
    
    //PUT Avatar
    [Authorize]
    [HttpPut("putAvatar")]
    public async Task<ActionResult> UpdateAvatar(/*[FromForm]*/ IFormFile file)
    {
        if(!file.ContentType.Contains("image"))
            return BadRequest("File is not an image");
        
        var userId =  Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return BadRequest("User not found");

        var stream = file.OpenReadStream();
        await using (stream.ConfigureAwait(false))
        using (var skBitmap = SKBitmap.Decode(stream))
        using (var image = SKImage.FromBitmap(skBitmap))
        using (var ms = new MemoryStream())
        {
            user.Avatar = image.Encode(SKEncodedImageFormat.Webp, 100).ToArray();
            await db.SaveChangesAsync();
        }
        
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpGet("{userId:guid}/tasks")]
    public Task<IQueryable<Card>> GetTasks(Guid userId)
    {
        return Task.FromResult(db.Cards.Where(c => c.AssignedUsers.Contains(userId)));
    }
}