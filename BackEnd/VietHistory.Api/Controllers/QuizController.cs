using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VietHistory.Application.DTOs;
using VietHistory.Application.Services;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/quiz")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpPost("create")]
    public async Task<ActionResult<QuizDto>> CreateQuiz([FromBody] CreateQuizRequest req)
    {
        // Try to get user ID, but allow anonymous users too
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.Identity?.Name;
        
        // If no authenticated user, use a consistent guest ID based on machine ID
        if (string.IsNullOrEmpty(userId))
        {
            // Try to get machine ID from request header or use a fallback
            var machineId = Request.Headers["X-Machine-Id"].FirstOrDefault() ?? "default";
            userId = $"guest_{machineId}";
        }

        var quiz = await _quizService.CreateQuizAsync(userId, req);
        return Ok(quiz);
    }

    [HttpGet("{quizId}")]
    public async Task<ActionResult<QuizDto>> GetQuiz(string quizId)
    {
        try
        {
            var quiz = await _quizService.GetQuizAsync(quizId);
            return Ok(quiz);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("submit")]
    public async Task<ActionResult<QuizAttemptDto>> SubmitQuiz([FromBody] SubmitQuizRequest req)
    {
        // Allow anonymous submissions
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.Identity?.Name;
        
        // If no authenticated user, use a consistent guest ID based on machine ID
        if (string.IsNullOrEmpty(userId))
        {
            var machineId = Request.Headers["X-Machine-Id"].FirstOrDefault() ?? "default";
            userId = $"guest_{machineId}";
        }

        try
        {
            var attempt = await _quizService.SubmitQuizAsync(userId, req);
            return Ok(attempt);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("my-quizzes")]
    public async Task<ActionResult<IReadOnlyList<QuizDto>>> GetMyQuizzes()
    {
        // Allow anonymous users
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.Identity?.Name;
        
        // If no authenticated user, use machine ID to find quizzes
        if (string.IsNullOrEmpty(userId))
        {
            var machineId = Request.Headers["X-Machine-Id"].FirstOrDefault() ?? "default";
            userId = $"guest_{machineId}";
        }

        var quizzes = await _quizService.GetUserQuizzesAsync(userId);
        return Ok(quizzes);
    }

    [HttpGet("{quizId}/my-attempt")]
    [Authorize]
    public async Task<ActionResult<QuizAttemptDto>> GetMyAttempt(string quizId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var attempt = await _quizService.GetUserQuizAttemptAsync(quizId, userId);
        if (attempt == null)
            return NotFound();

        return Ok(attempt);
    }
}

