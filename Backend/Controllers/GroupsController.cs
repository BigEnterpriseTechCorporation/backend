using System.Security.Claims;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

public class AddCardToGroupInBoard
{
    public string Title { get; set; }
    public Guid GroupId { get; set; }
    public string Content { get; set; }
    public List<Guid> AssignedUsers { get; set; }
}


[ApiController]
[Route("api/[controller]")]
public class GroupController(AppDbContext db) : ControllerBase
{
    [Authorize]
    [HttpPost("{id:guid}/add/card")]
    public async Task<IActionResult> AddCardToGroupInBoard(Guid id, [FromBody]AddCardToGroupInBoard request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        
        var board = await db.Boards.SingleOrDefaultAsync(x => x.Id == id);
        
        if(!db.Groups.Any(g => g.Id == request.GroupId)) return NotFound();

        var c = new Card()
        {
            Id = Guid.NewGuid(),
            Name = request.Title,
            GroupId = request.GroupId,
            Content = request.Content,
            AssignedUsers = request.AssignedUsers,
            CreatedAt = DateTime.Now
        };
        
        
        
        return Ok();
    }
}