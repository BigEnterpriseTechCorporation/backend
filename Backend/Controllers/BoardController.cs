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
    //GET Board by ID
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Board>> GetBoard(Guid id)
    {
        var board = await db.Boards.SingleAsync(x => x.Id == id);

        if (board == null)
            return NotFound();

        return board;
    }

    //GET All Boards
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Board>>> GetBoards()
    {
        return await db.Boards.ToListAsync();
    }
    
    //CREATE Board
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

    //ADD User to board by boardID
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
        if (board == null)
        {
            return BadRequest();
        }

        if (!board.Admins.Contains(userId))
        {
            return BadRequest();
        }
        var addUser = await db.Users.SingleAsync(x => x.Login == request.Login);
        if (board.Users.Contains(addUser.Id))
        {
            return BadRequest();
        }
        if (request.AsAdmin)
        {
            board?.Admins.Add(userId);
        }
        board?.Users.Add(addUser.Id);
        
        addUser.Boards.Add(id);

        await db.SaveChangesAsync();
        
        return Ok();
    }
    
    //REMOVE User by ID from Board by ID
    [Authorize]
    [HttpDelete("{id:guid}/remove/user/{userId:guid}")]
    public async Task<IActionResult> RemoveUserFromBoard(Guid id, Guid userId)
    {
        var localUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        
        var board = await db.Boards.SingleOrDefaultAsync(x => x.Id == id);

        if (board == null & !board.Admins.Contains(localUserId) & !board.Users.Contains(userId))
        {
            return BadRequest();
        }
        
        board.Admins.Remove(userId);
        board.Users.Remove(userId);
        (await db.Users.FindAsync(userId))?.Boards.Remove(id);
        
        await db.SaveChangesAsync();
        
        return Ok();
    }
    
    //REMOVE Group from Board
    [Authorize]
    [HttpDelete("{id:guid}/remove/group/{groupId:guid}")]
    public async Task<IActionResult> RemoveGroupFromBoard(Guid id, Guid groupId)
    {
        var localUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        
        var board = await db.Boards.SingleOrDefaultAsync(x => x.Id == id);
        
        if(board == null) return BadRequest();
        if (!board.Admins.Contains(localUserId) & !board.Groups.Contains(groupId)) return BadRequest();
        
        board.Groups.Remove(groupId);
        var g = await db.Groups.FindAsync(groupId);
        if(g != null) db.Groups.Remove(g);
        
        await db.SaveChangesAsync();
        
        return Ok();
    }
    
    //ADD Group
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

        if(board == null) return BadRequest();
        if (!board.Admins.Contains(userId)) return BadRequest();
        
        var newGroup = new Group
        {
            Name = request.Name,
            CreatedAt = DateTime.Now,
            BoardId = board.Id 
        };
        
        db.Groups.Add(newGroup);
        board.Groups.Add(newGroup.Id);

        await db.SaveChangesAsync();
        
        return Ok(newGroup.Id.ToString("D"));
    }
    
    //GET Group
    [AllowAnonymous]
    [HttpGet("{id:guid}/get/group")]
    public async Task<IActionResult> GetGroupOnBoard(Guid id)
    {
        return Ok(await db.Groups.FindAsync(id));
    }
    
}