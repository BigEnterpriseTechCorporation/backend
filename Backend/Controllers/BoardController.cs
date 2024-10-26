using System.Security.Claims;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

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
    [HttpPost("create")]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);
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
        user.Boards.Add(board.Id);

        await db.SaveChangesAsync();

        return Ok(board.Id);
    }

    //ADD User to board by boardID
    [Authorize]
    [HttpPost("{id:guid}/add/user")]
    public async Task<IActionResult> AddUserToBoard(Guid id, [FromBody] AddUserBoardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);

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
            board?.Admins.Add(addUser.Id);
        }

        board?.Users.Add(addUser.Id);

        //addUser.Boards.Add(id);
        
        (await db.Users.FindAsync(addUser.Id))?.Boards.Add(id);

        await db.SaveChangesAsync();

        return Ok();
    }

    //REMOVE User by ID from Board by ID
    [Authorize]
    [HttpDelete("{id:guid}/remove/user/{userId:guid}")]
    public async Task<IActionResult> RemoveUserFromBoard(Guid id, Guid userId)
    {
        var localUserId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);

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
        var localUserId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);

        var board = await db.Boards.SingleOrDefaultAsync(x => x.Id == id);

        if (board == null) return BadRequest();
        if (!board.Admins.Contains(localUserId) & !board.Groups.Contains(groupId)) return BadRequest();

        board.Groups.Remove(groupId);
        var g = await db.Groups.FindAsync(groupId);
        if (g != null) db.Groups.Remove(g);

        await db.SaveChangesAsync();

        return Ok();
    }

    //ADD Group
    [Authorize]
    [HttpPost("{id:guid}/add/group")]
    public async Task<IActionResult> AddGroupToBoard(Guid id, [FromBody] AddGroupToBoardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId =
            Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ??
                       string.Empty);

        var board = await db.Boards.SingleOrDefaultAsync(x => x.Id == id);

        if (board == null) return BadRequest();
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

    /*[AllowAnonymous]
    [HttpGet("{id:guid}/search")]
    public ActionResult Search(Guid id, string query)
    {
        var b = db.Boards.Find(id);
        if(b == null) return NotFound();
        List<Guid> ids = [];
        List<Guid> fids = [];
        foreach (var g in b.Groups)
        {
            var gr = db.Groups.Find(g);
            if(gr == null) return NotFound();
            ids.AddRange(gr.Cards);
        }

        fids.AddRange(ids.Where(i => db.Cards.Find(i)!.Name.Contains(query)));
        return new JsonResult(fids);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/filter")]
    public ActionResult Filter(Guid id, string who, DateTime from, DateTime to)
    {
        var b = db.Boards.Find(id);
        if (b == null) return NotFound();
        List<Guid> ids = [];
        List<Guid> fids = [];
        foreach (var g in b.Groups)
        {
            var gr = db.Groups.Find(g);
            if (gr == null) return NotFound();
            ids.AddRange(gr.Cards);
        }

        foreach (var i in ids)
        {
            var c = db.Cards.Find(i);
            
        }

    //fids.AddRange(ids.Where(i => db.Cards.Find(i)!.AssignedUsers.Contains()));
    return new JsonResult(fids);
    }*/

    [AllowAnonymous]
    [HttpGet("{id:guid}/export")]
    public async Task<ActionResult<FileResult>> GetBoardExport(Guid id)
    {
        var board = await db.Boards.FindAsync(id);
        if (board == null) return NotFound();
        var groups = await db.Groups.ToListAsync();
        if (groups.Count == 0) return NotFound();

        var cont = new List<CardExport>();
        foreach (var group in groups)
        {
            foreach (var groupCard in group.Cards)
            {
                var card = await db.Cards.FindAsync(groupCard);
                if (card == null) continue;
                //var u = (await db.Users.Where())?.Name;
                //if (u == null) continue;
                
                /*var ec = new CardExport
                {
                    Name = card.Name,
                    Content = card.Content,
                    AssignedUsers = u,
                    
                };*/
            }
        }
        
        return Ok();
    }
}