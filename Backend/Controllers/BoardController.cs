using System.Security.Claims;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

public class CreateBoardRequest
{
    public string Name { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class BoardController(AppDbContext db) : ControllerBase
{
    /*[HttpGet("{id:guid}")]
    public async Task<ActionResult<Board>> Get(Guid id)
    {
        var board = await db.Boards.FirstOrDefaultAsync(x => x.Id == id);
        if (board == null)
            return NotFound();
        return new ObjectResult(board.Name);
    }*/
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Board>> GetBoard(Guid id)
    {
        var board = await db.Boards.SingleAsync(x => x.Id == id);

        if (board == null)
            return NotFound();

        return board;
    }

    [AllowAnonymous]
    [HttpGet()]
    public async Task<ActionResult<IEnumerable<Board>>> GetBoards()
    {
        return await db.Boards.ToListAsync();
    }
    
    /*[Authorize]
    [HttpPost("create")]
    public async Task<ActionResult<Board>> Create([FromBody] string name)
    {
        var board = new Board
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        db.Boards.Add(board);
        await db.SaveChangesAsync(); // Save the board first to generate the ID
        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);
        // Now, add the user as an admin of the newly created board
        var boardUser = new BoardUser
        {
            BoardId = board.Id,
            UserId = userId,
            Role = BoardRole.Admin // Assign the user as an admin
        };
       // boardUser.BoardId = board.Id;

        db.BoardUsers.Add(boardUser);
        await db.SaveChangesAsync();
        
        
        db.Boards.SingleOrDefault(x => x.Id == board.Id).BoardUsers.Add(boardUser);
        db.Users.Single(x => x.Id == userId).BoardUsers.Add(boardUser);
        
        await db.SaveChangesAsync(); // Save the board-user association

        return Ok(board.Id); //CreatedAtAction(nameof(GetBoard), new { id = board.Id }, board);

    }*/
    /*[Authorize]
    [HttpPost("create")]
    public async Task<ActionResult<Board>> Create([FromBody] string name)
    {
        var board = new Board
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        db.Boards.Add(board);
        await db.SaveChangesAsync(); // Save the board first to generate the ID

        var userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var user = await db.Users.FindAsync(userId);

        var boardUser = new BoardUser
        {
            Board = board,
            User = user,
            Role = BoardRole.Admin
        };

        // Add the BoardUser to the Board's BoardUsers collection
        //board.BoardUsers.Add(boardUser);

        // Add the BoardUser to the User's BoardUsers collection
        //user.BoardUsers.Add(boardUser);

        await db.SaveChangesAsync(); // Save all changes (including BoardUser associations)

        return Ok(board.Id);
    }*/
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
            Creator = user
        };
        var bid = board.Id;
        db.Boards.Add(board);
        await db.SaveChangesAsync();

        (await db.Boards.FindAsync(bid))
            ?.BoardUsers.Add(new BoardUser
        {
            Board = board,
            User = user,
            Role = BoardRole.Admin
        });

        
        await db.SaveChangesAsync();

        return Ok(new { Id = board.Id });
    }
}