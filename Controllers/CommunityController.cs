using AgriSmartAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.Controllers;

[ApiController]
[Route("api/community")]
// [Authorize] // Uncomment if you want to require authentication
public class CommunityController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public CommunityController(AgriSmartContext context)
    {
        _context = context;
    }

    // ========== Forum ==========
    // GET: api/community/forum?query=...
    [HttpGet("forum")]
    public async Task<IActionResult> GetForumPosts([FromQuery] string? query)
    {
        try
        {
            // NOTE: Adjust property names to your ForumPost model if needed
            var postsQuery = _context.Set<Models.ForumPost>()
                .AsNoTracking()
                .Include(p => p.Comments) // assumes navigation property exists
                .OrderByDescending(p => p.PostedDate)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                postsQuery = postsQuery.Where(p =>
                    (p.Title ?? "").ToLower().Contains(q) ||
                    (p.Content ?? "").ToLower().Contains(q) ||
                    (p.Tags ?? "").ToLower().Contains(q));
            }

            var posts = await postsQuery
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    content = p.Content,
                    likes = p.Likes,
                    views = p.Views,
                    tags = (p.Tags ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    author = new { name = p.AuthorName },
                    time = p.PostedDate,
                    comments = p.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new { id = c.Id, text = c.Text, createdAt = c.CreatedAt })
                        .ToList()
                })
                .ToListAsync();

            return Ok(posts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load forum posts.", detail = ex.Message });
        }
    }

    // POST: api/community/forum
    [HttpPost("forum")]
    public async Task<IActionResult> CreateForumPost([FromBody] CreateForumPostRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var entity = new Models.ForumPost
            {
                Title = request.Title.Trim(),
                Content = request.Content.Trim(),
                PostedDate = DateTime.UtcNow,
                Likes = 0,
                Views = 0,
                Tags = request.Tags is { Count: > 0 }
                    ? string.Join(",", request.Tags)
                    : null,
                AuthorName = request.AuthorName ?? "Farmer"
            };

            _context.Set<Models.ForumPost>().Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetForumPostById), new { id = entity.Id }, new { id = entity.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to create forum post.", detail = ex.Message });
        }
    }

    // GET: api/community/forum/{id}
    [HttpGet("forum/{id:int}")]
    public async Task<IActionResult> GetForumPostById([FromRoute] int id)
    {
        var post = await _context.Set<Models.ForumPost>()
            .AsNoTracking()
            .Include(p => p.Comments)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                id = p.Id,
                title = p.Title,
                content = p.Content,
                likes = p.Likes,
                views = p.Views,
                tags = (p.Tags ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                author = new { name = p.AuthorName },
                time = p.PostedDate,
                comments = p.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new { id = c.Id, text = c.Text, createdAt = c.CreatedAt })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (post is null) return NotFound(new { message = "Post not found." });
        return Ok(post);
    }

    // POST: api/community/forum/{id}/like
    [HttpPost("forum/{id:int}/like")]
    public async Task<IActionResult> LikePost([FromRoute] int id)
    {
        var post = await _context.Set<Models.ForumPost>().FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return NotFound(new { message = "Post not found." });

        try
        {
            post.Likes = (post.Likes) + 1;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Liked", likes = post.Likes });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to like post.", detail = ex.Message });
        }
    }

    // POST: api/community/forum/{id}/comment
    [HttpPost("forum/{id:int}/comment")]
    public async Task<IActionResult> AddComment([FromRoute] int id, [FromBody] AddCommentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var post = await _context.Set<Models.ForumPost>()
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null) return NotFound(new { message = "Post not found." });

        try
        {
            var comment = new Models.ForumComment
            {
                PostId = id,
                Text = request.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            // If Comments is a navigation collection
            post.Comments ??= new List<Models.ForumComment>();
            post.Comments.Add(comment);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Comment added." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to add comment.", detail = ex.Message });
        }
    }

    // ========== Experts ==========
    // GET: api/community/experts
    [HttpGet("experts")]
    public async Task<IActionResult> GetExperts()
    {
        try
        {
            // Adjust entity and mapping to match your model
            var experts = await _context.Set<Models.Expert>()
                .AsNoTracking()
                .OrderByDescending(e => e.Rating)
                .Select(e => new
                {
                    id = e.Id,
                    name = e.Name,
                    specialty = e.Specialty,
                    rating = e.Rating,
                    answers = e.Answers,
                    badges = e.Badges // if stored as CSV, split here
                })
                .ToListAsync();

            return Ok(experts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load experts.", detail = ex.Message });
        }
    }
    
    [HttpPost("experts")]
    public async Task<IActionResult> AddExpert([FromBody] CreateExpertRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var entity = new Models.Expert
            {
                Name = request.Name.Trim(),
                Specialty = string.IsNullOrWhiteSpace(request.Specialty) ? null : request.Specialty.Trim(),
                Rating = request.Rating ?? 4.5,
                Answers = request.Answers ?? 0,
                Badges = request.Badges is { Count: > 0 } ? string.Join(",", request.Badges) : null
            };

            _context.Experts.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExpertById), new { id = entity.Id }, new
            {
                id = entity.Id,
                name = entity.Name,
                specialty = entity.Specialty,
                rating = entity.Rating,
                answers = entity.Answers,
                badges = entity.Badges
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to add expert.", detail = ex.Message });
        }
    }

    // GET: api/community/experts/{id}
    [HttpGet("experts/{id:int}")]
    public async Task<IActionResult> GetExpertById([FromRoute] int id)
    {
        var e = await _context.Experts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NotFound(new { message = "Expert not found." });

        return Ok(new
        {
            id = e.Id,
            name = e.Name,
            specialty = e.Specialty,
            rating = e.Rating,
            answers = e.Answers,
            badges = e.Badges
        });
    }


    // ========== Tutorials ==========
    // GET: api/community/tutorials?query=...
    [HttpGet("tutorials")]
    public async Task<IActionResult> GetTutorials([FromQuery] string? query)
    {
        try
        {
            // Adjust entity and mapping to match your model
            var tutorialsQuery = _context.Set<Models.Tutorial>()
                .AsNoTracking()
                .OrderByDescending(t => t.PublishedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                tutorialsQuery = tutorialsQuery.Where(t =>
                    (t.Title ?? "").ToLower().Contains(q) ||
                    (t.Description ?? "").ToLower().Contains(q) ||
                    (t.Category ?? "").ToLower().Contains(q));
            }

            var tutorials = await tutorialsQuery
                .Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    description = t.Description,
                    duration = t.Duration, // e.g., "10:00"
                    views = t.Views,
                    category = t.Category,
                    difficulty = t.Difficulty,
                    publishedAt = t.PublishedAt,
                    videoPath = t.VideoPath,
                    streamUrl = "/" + (t.VideoPath ?? string.Empty).TrimStart('/'),
                    downloadUrl = $"/community/tutorials/download/{t.Id}"
                })
                .ToListAsync();

            return Ok(tutorials);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load tutorials.", detail = ex.Message });
        }
    }
    // GET: api/community/tutorials/download/{id}
    [HttpGet("tutorials/download/{id:int}")]
    public async Task<IActionResult> DownloadTutorial([FromRoute] int id, [FromServices] IWebHostEnvironment env)
    {
        var tutorial = await _context.Tutorials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (tutorial is null) return NotFound(new { message = "Tutorial not found." });

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var filePath = Path.Combine(webRoot, tutorial.VideoPath);

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { message = "Video file not found." });

        var fileName = Path.GetFileName(tutorial.VideoPath);
        var contentType = "application/octet-stream";

        return PhysicalFile(filePath, contentType, fileName);
    }

    [HttpPost("tutorials/post")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateTutorial([FromForm] CreateTutorialForm form, [FromServices] IWebHostEnvironment env)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (form.VideoFile is null || form.VideoFile.Length == 0)
            return BadRequest(new { message = "Video file is required." });

        // Validate content type and extension (basic)
        var allowedExtensions = new[] { ".mp4", ".mov", ".webm", ".mkv" };
        var ext = Path.GetExtension(form.VideoFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(new { message = $"Unsupported file type. Allowed: {string.Join(", ", allowedExtensions)}" });

        // Optionally validate size (example: < 200 MB)
        const long maxBytes = 200L * 1024L * 1024L;
        if (form.VideoFile.Length > maxBytes)
            return BadRequest(new { message = "File too large. Max 200 MB." });

        // Prepare folder and filename
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var videosDir = Path.Combine(webRoot, "videos");
        if (!Directory.Exists(videosDir))
            Directory.CreateDirectory(videosDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(videosDir, fileName);
        var relativePath = Path.Combine("videos", fileName).Replace("\\", "/"); // stored in DB

        // Save file
        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await form.VideoFile.CopyToAsync(stream);
        }

        // Create entity
        var entity = new AgriSmartAPI.Models.Tutorial
        {
            Title = form.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
            Duration = string.IsNullOrWhiteSpace(form.Duration) ? null : form.Duration.Trim(),
            Category = string.IsNullOrWhiteSpace(form.Category) ? null : form.Category.Trim(),
            Difficulty = string.IsNullOrWhiteSpace(form.Difficulty) ? null : form.Difficulty.Trim(),
            PublishedAt = DateTime.UtcNow,
            VideoPath = relativePath
        };

        _context.Tutorials.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTutorialById), new { id = entity.Id }, new
        {
            id = entity.Id,
            title = entity.Title,
            description = entity.Description,
            duration = entity.Duration,
            category = entity.Category,
            difficulty = entity.Difficulty,
            videoPath = entity.VideoPath
        });
    }

    [HttpGet("tutorials/{id:int}")]
    public async Task<IActionResult> GetTutorialById([FromRoute] int id)
    {
        var t = await _context.Tutorials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound(new { message = "Tutorial not found." });

        return Ok(new
        {
            id = t.Id,
            title = t.Title,
            description = t.Description,
            duration = t.Duration,
            category = t.Category,
            difficulty = t.Difficulty,
            views = t.Views,
            publishedAt = t.PublishedAt,
            videoPath = t.VideoPath
        });
    }


    // Optional: increment views when a tutorial is opened
    // POST: api/community/tutorials/{id}/view
    [HttpPost("tutorials/{id:int}/view")]
    public async Task<IActionResult> IncrementTutorialView([FromRoute] int id)
    {
        var tutorial = await _context.Set<Models.Tutorial>().FirstOrDefaultAsync(t => t.Id == id);
        if (tutorial is null) return NotFound(new { message = "Tutorial not found." });

        try
        {
            tutorial.Views = (tutorial.Views) + 1;
            await _context.SaveChangesAsync();
            return Ok(new { message = "View incremented.", views = tutorial.Views });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update views.", detail = ex.Message });
        }
    }

    // ========== DTOs ==========
    public class CreateExpertRequest
    {
        [Required, MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(120)]
        public string? Specialty { get; set; }

        [Range(0, 5)]
        public double? Rating { get; set; }

        [Range(0, int.MaxValue)]
        public int? Answers { get; set; }

        public List<string>? Badges { get; set; }
    }

    public class CreateForumPostRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        public List<string>? Tags { get; set; }

        public string? AuthorName { get; set; }
    }
    public class CreateTutorialForm
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(16)]
        public string? Duration { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(50)]
        public string? Difficulty { get; set; }

        [Required]
        public IFormFile? VideoFile { get; set; }
    }

    public class AddCommentRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}