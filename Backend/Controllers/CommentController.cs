using System.Security.Claims;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

public class CommentController(AppDbContext db) : ControllerBase
{
    [Authorize]
    [HttpPost("{cardId:guid}/comment")]
    public async Task<ActionResult> CreateComment(Guid cardId, [FromBody]CommentRequest com)
    {
        if (!ModelState.IsValid) return BadRequest();
        
        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);
        var card = await db.Cards.FindAsync(cardId);
        if(card == null) return NotFound();
        var c = new Comment
        {
            Id = Guid.NewGuid(),
            Card = card.Id,
            CreatedAt = DateTime.Now,
            Content = com.Content,
            Owner = userId
        };
        card.Comments.Add(c.Id);
        db.Comments.Add(c);

        await db.SaveChangesAsync();

        return Ok(c.Id);
    }
    
    [Authorize]
    [HttpDelete("{commentId:guid}/comment")]
    public async Task<ActionResult> DeleteComment(Guid commentId)
    {
        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);
        
        var c = await db.Comments.FindAsync(commentId);
        if(c == null) return NotFound();
        var card = await db.Cards.FindAsync(c.Card);
        if(card == null) return NotFound();
        
        card.Comments.Remove(c.Id);
        db.Comments.Remove(c);
        
        await db.SaveChangesAsync();

        return Ok();
    }
}