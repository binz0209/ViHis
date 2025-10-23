using Microsoft.AspNetCore.Mvc;
using VietHistory.Application.DTOs;
using VietHistory.Application.Services;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
public class AiController : ControllerBase
{
    private readonly IAIStudyService _ai;
    public AiController(IAIStudyService ai) => _ai = ai;

    [HttpPost("ask")]
    public async Task<ActionResult<AiAnswer>> Ask([FromBody] AiAskRequest req)
    {
        var res = await _ai.AskAsync(req);
        return Ok(res);
    }
}
