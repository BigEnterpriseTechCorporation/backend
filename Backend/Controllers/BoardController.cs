using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardController(AppDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Board>> Get(Guid id)
    {
        var board = await db.Boards.FirstOrDefaultAsync(x => x.Id == id);
        if (board == null)
            return NotFound();
        return new ObjectResult(board.Name);
    }
    
    [Authorize]
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] string name)
    {
        var board = new Board
        {
            Name = name,
            CreatedAt = DateTime.Now,
        };
        
       var r = db.Add(board);
       await db.SaveChangesAsync();
       return Ok(r.Entity.Id);
    }
}