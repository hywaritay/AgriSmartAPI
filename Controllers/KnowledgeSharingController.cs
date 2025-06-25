using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriSmartAPI.Controllers;

[Route("api/knowledge")]
[ApiController]
//[Authorize]
public class KnowledgeSharingController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public KnowledgeSharingController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpPost("forumpost")]
    public async Task<ActionResult<ForumPost>> CreateForumPost([FromBody] ForumPost post)
    {
        post.PostedDate = DateTime.Now;
        _context.ForumPosts.Add(post);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetForumPost), new { id = post.Id }, post);
    }

    [HttpGet("forumpost/{id}")]
    public async Task<ActionResult<ForumPost>> GetForumPost(int id)
    {
        var post = await _context.ForumPosts.FindAsync(id);
        if (post == null) return NotFound();
        return Ok(post);
    }

    [HttpPost("forumpost/{id}/reply")]
    public async Task<ActionResult<ForumPost>> AddReply(int id, [FromBody] ReplyRequest replyRequest)
    {
        var post = await _context.ForumPosts.FindAsync(id);
        if (post == null) return NotFound();

        post.Replies.Add(replyRequest.Reply);
        await _context.SaveChangesAsync();
        return Ok(post);
    }

    [HttpPost("expertqa")]
    public async Task<ActionResult<ExpertQA>> SubmitQuestion([FromBody] ExpertQA qa)
    {
        qa.Answer = "Sample answer from expert";
        qa.ExpertId = "E123";
        qa.AnsweredDate = DateTime.Now;
        _context.ExpertQAs.Add(qa);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetExpertQA), new { id = qa.Id }, qa);
    }

    [HttpGet("expertqa/{id}")]
    public async Task<ActionResult<ExpertQA>> GetExpertQA(int id)
    {
        var qa = await _context.ExpertQAs.FindAsync(id);
        if (qa == null) return NotFound();
        return Ok(qa);
    }
}