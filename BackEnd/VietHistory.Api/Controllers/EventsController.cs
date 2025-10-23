using Microsoft.AspNetCore.Mvc;
using VietHistory.Application.DTOs;
using VietHistory.Application.Services;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
public class EventsController : ControllerBase
{
    private readonly IEventsService _service;
    public EventsController(IEventsService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> List([FromQuery] int? month, [FromQuery] int? day, [FromQuery] string? q, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var res = await _service.ListAsync(month, day, q, skip, take);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventRequest req)
    {
        var res = await _service.CreateAsync(req);
        return CreatedAtAction(nameof(List), new { id = res.Id }, res);
    }
}
