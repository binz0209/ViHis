using Microsoft.AspNetCore.Mvc;
using VietHistory.Application.DTOs;
using VietHistory.Application.Services;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/people")]
public class PeopleController : ControllerBase
{
    private readonly IPeopleService _service;
    public PeopleController(IPeopleService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PersonDto>>> List([FromQuery] string? q, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var res = await _service.ListAsync(q, skip, take);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<PersonDto>> Create([FromBody] CreatePersonRequest req)
    {
        var res = await _service.CreateAsync(req);
        return CreatedAtAction(nameof(List), new { id = res.Id }, res);
    }
}
