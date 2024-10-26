using System.Security.Claims;
using Azure.Core;
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
    public string Content { get; set; }
    public List<Guid> AssignedUsers { get; set; }
}

public class RenameGroup
{
    public string Name { get; set; }
}


[ApiController]
[Route("api/[controller]")]
public class GroupController(AppDbContext db) : ControllerBase
{
    //CREATE Card
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

        var g = await db.Groups.FindAsync(id);
        
        if(g == null) return NotFound();

        var c = new Card
        {
            Id = Guid.NewGuid(),
            Name = request.Title,
            GroupId = id,
            Content = request.Content,
            AssignedUsers = request.AssignedUsers,
            CreatedAt = DateTime.Now
        };
        g.Cards.Add(c.Id);
        db.Cards.Add(c);
        
        await db.SaveChangesAsync();
        
        return Ok();
    }
    
    //EDIT Card
    [Authorize]
    [HttpPost("{id:guid}/edit/card")]
    public async Task<IActionResult> EditCard(Guid id, [FromBody]AddCardToGroupInBoard request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var c = await db.Cards.SingleOrDefaultAsync(x => x.Id == id);
        
        c.Content = request.Content;
        c.Name = request.Title;
        c.AssignedUsers = request.AssignedUsers;
        
        await db.SaveChangesAsync();
        
        return Ok();
    }
    
    //MOVE Card
    [Authorize]
    [HttpPost("{id:guid}/edit/card")]
    public async Task<IActionResult> MoveCard(Guid id, Guid newGroupId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var c = await db.Cards.SingleOrDefaultAsync(x => x.Id == id);
       
        var og = await db.Groups.FindAsync(c.GroupId);
        og.Cards.Remove(c.Id);
        c.GroupId = newGroupId;
        var ng = await db.Groups.FindAsync(newGroupId);
        ng.Cards.Add(c.Id);
        
        await db.SaveChangesAsync();
        
        return Ok();
    }
    
    //Get Cards ids
    [AllowAnonymous]
    [HttpPost("{id:guid}/get/ids")]
    public async Task<IActionResult> GetCardsFromGroupInBoard(Guid id)
    {
        var g = await db.Groups.FindAsync(id);
        
        if(g == null) return NotFound();
        
        return Ok(g.Cards.ToList());
    }
    
    //Get Cards ids
    [AllowAnonymous]
    [HttpPost("{id:guid}/get/full")]
    public async Task<IActionResult> GetFullCardsFromGroupInBoard(Guid id)
    {
        var g = await db.Groups.FindAsync(id);
        
        if(g == null) return NotFound();
        
        //! POSSIBLE PERFORMANCE ISSUES
        return Ok(db.Cards.Where(t => g.Cards.Contains(t.Id))); 
    }
    
    //RENAME group
    [Authorize]
    [HttpPost("{id:guid}/rename")]
    public async Task<IActionResult> RenameGroup(Guid id, [FromBody] RenameGroup req)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);
        
        var userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        
        var g = await db.Groups.FindAsync(id);
        
        var board = await db.Boards.SingleOrDefaultAsync(x => g != null && x.Id == g.BoardId);

        if(board == null | g == null) return BadRequest();
        if (board != null && !board.Admins.Contains(userId)) return BadRequest();

        if (g != null) g.Name = req.Name;

        await db.SaveChangesAsync();
        return Ok();
    }
}