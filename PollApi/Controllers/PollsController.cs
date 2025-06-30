using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollLibrary.DataAccess;
using PollLibrary.Models;

[ApiController]
[Route("api/[controller]")]
public class PollsController : ControllerBase
{
    private readonly IPollRepository _repo;
    public PollsController(IPollRepository repo) => _repo = repo;

    // only authenticated users
    [HttpPost, Authorize]
    public async Task<ActionResult<Poll>> CreatePoll([FromBody] Poll poll)
    {
        if (poll.Options.Count < 2 || poll.Options.Count > 5)
            return BadRequest("Must have 2–5 options.");

        poll.Id = Guid.NewGuid();
        await _repo.CreatePollAsync(poll);

        var created = await _repo.GetPollAsync(poll.Id);
        return CreatedAtAction(nameof(GetPoll), new { id = poll.Id }, created);
    }

    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PollSummary>>> GetPollList()
    {
        var list = await _repo.GetPollSummariesAsync();
        return Ok(list);
    }

    // public view
    [HttpGet("{id:guid}"), AllowAnonymous]
    public async Task<ActionResult<Poll>> GetPoll(Guid id)
    {
        var poll = await _repo.GetPollAsync(id);
        return poll is null ? NotFound() : Ok(poll);
    }

    // public vote
    [HttpPost("{id:guid}/vote"), AllowAnonymous]
    public async Task<ActionResult<Poll>> Vote(Guid id, [FromBody] int optionId)
    {
        var poll = await _repo.GetPollAsync(id);
        if (poll == null || !poll.Options.Any(o => o.Id == optionId))
            return BadRequest("Invalid poll or option.");

        await _repo.VoteAsync(id, optionId);
        return await GetPoll(id);
    }
}
