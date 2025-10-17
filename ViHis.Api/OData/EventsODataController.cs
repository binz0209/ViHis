
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ViHis.Infrastructure.Mongo;
using ViHis.Domain.Entities;
using MongoDB.Driver;

namespace ViHis.Api.Controllers.OData;

[Route("odata/[controller]")]
public class EventsController : ODataController
{
    private readonly IMongoDbContext _ctx;
    public EventsController(IMongoDbContext ctx) { _ctx = ctx; }

    [EnableQuery]
    public IActionResult Get() => Ok(_ctx.Events.AsQueryable());

    [EnableQuery]
    public IActionResult Get([FromRoute] string key) => Ok(_ctx.Events.AsQueryable().Where(e => e.Id == key));
}
