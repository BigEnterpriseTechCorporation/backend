using System.Security.Claims;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

public class CreateBoardRequest
{
    public string Name { get; set; }
}
public class AddUserBoardRequest
{
    public string Login { get; set; }
    public bool AsAdmin { get; set; }
}
public class AddGroupToBoardRequest
{
    public string Name { get; set; }
}


[ApiController]
[Route("api/[controller]")]
public class BoardController(AppDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Board>> GetBoard(Guid id)
    {
        var board = await db.Boards.SingleAsync(x => x.Id == id);

        if (board == null)
            return NotFound();

        return board;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Board>>> GetBoards()
    {
        return await db.Boards.ToListAsync();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var user = await db.Users.FindAsync(userId);

        if (user == null)
        {
            return BadRequest("User not found");
        }

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.Now
        };
        board.Users.Add(userId);
        board.Admins.Add(userId);
        db.Boards.Add(board);
        
        await db.SaveChangesAsync();

        return Ok(new { Id = board.Id });
    }

    [Authorize]
    [HttpPost("{id:guid}/add/user")]
    public async Task<IActionResult> AddUserToBoard(Guid id, [FromBody]AddUserBoardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        
        var board = await db.Boards.SingleOrDefaultAsync(x => x.Id == id);

        if (board != null && !board.Admins.Contains(userId))
        {
            return BadRequest();
        }
        var addUser = await db.Users.SingleAsync(x => x.Login == request.Login);
        if (request.AsAdmin)
        {
            board?.Admins.Add(userId);
        }
        board?.Users.Add(addUser.Id);

        await db.SaveChangesAsync();
        
        return Ok();
    }
    
    [Authorize]
    [HttpPost("{id:guid}/add/group")]
    public async Task<IActionResult> AddGroupToBoard(Guid id, [FromBody]AddGroupToBoardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        
        var board = await db.Boards.SingleOrDefaultAsync(x => x.Id == id);

        if (board != null && !board.Admins.Contains(userId))
        {
            return BadRequest();
        }

        if (board != null)
        {
            var newGroup = new Group()
            {
                Name = request.Name,
                CreatedAt = DateTime.Now,
                BoardId = board.Id
            };
            db.Groups.Add(newGroup);
            board?.Groups.Add(newGroup.Id);
        }

        await db.SaveChangesAsync();
        
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("{id:guid}/get/group")]
    public async Task<IActionResult> GetGroupOnBoard(Guid id)
    {
        return Ok(await db.Groups.FindAsync(id));
    }
    
}