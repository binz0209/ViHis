using Microsoft.AspNetCore.Mvc;
using VietHistory.Application.Services;
using VietHistory.Application.DTOs;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/today")]
public class TodayController : ControllerBase
{
    private readonly IEventsService _events;
    public TodayController(IEventsService events) => _events = events;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetToday()
    {
        var now = DateTime.UtcNow; // consider using VN timezone in frontend
        var res = await _events.ListAsync(now.Month, now.Day, null, 0, 50);
        return Ok(res);
    }
}
