
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ViHis.Application.DTOs;
using ViHis.Application.Services;
using ViHis.Domain.Entities;
using ViHis.Infrastructure.Mongo;
using MongoDB.Driver;

namespace ViHis.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/people")]
public class PeopleController : ControllerBase
{
    private readonly IPersonService _service;
    public PeopleController(IPersonService service) { _service = service; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonListDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<PersonDetailDto>> GetById(string id)
    {
        var person = await _service.GetByIdAsync(id);
        return person is null ? NotFound() : Ok(person);
    }

    [HttpPost]
    public async Task<ActionResult<PersonDetailDto>> Create([FromBody] CreatePersonDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id, version = "1.0" }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePersonDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}

[Route("odata/[controller]")]
public class PeopleControllerOData : ODataController
{
    private readonly ViHis.Infrastructure.Mongo.IMongoDbContext _ctx;
    public PeopleControllerOData(ViHis.Infrastructure.Mongo.IMongoDbContext ctx) { _ctx = ctx; }

    [EnableQuery]
    public IActionResult Get() => Ok(_ctx.People.AsQueryable());

    [EnableQuery]
    public IActionResult Get([FromRoute] string key) => Ok(_ctx.People.AsQueryable().Where(p => p.Id == key));
}
