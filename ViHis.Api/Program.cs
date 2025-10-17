using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.ModelBuilder;
using MongoDB.Driver;
using ViHis.Application.Services;
using ViHis.Domain.Entities;
using ViHis.Infrastructure.Mongo;
using ViHis.Infrastructure.Repositories;
using ViHis.Infrastructure.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Mongo
var cfg = builder.Configuration;
var mongoOpts = new MongoOptions
{
    ConnectionString = cfg.GetSection("Mongo:ConnectionString").Value ?? "",
    Database = cfg.GetSection("Mongo:Database").Value ?? "vihis"
};
builder.Services.AddSingleton<IMongoDbContext>(new MongoDbContext(mongoOpts));

// MVC + OData (AddOData phải chain sau AddControllers)
builder.Services
    .AddControllers()
    .AddOData(opt =>
    {
        opt.Select().Filter().OrderBy().Expand().SetMaxTop(100);
        opt.EnableQueryFeatures();
        opt.AddRouteComponents("odata", GetEdmModel());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(opt =>
{
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.ReportApiVersions = true;
});

// Repos & Services
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IPersonService, PersonService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

static Microsoft.OData.Edm.IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.Namespace = "ViHis";
    builder.ContainerName = "ViHisContainer";
    builder.EntitySet<Person>("People");
    builder.EntitySet<Event>("Events");
    builder.EntitySet<Dynasty>("Dynasties");
    builder.EntitySet<Period>("Periods");
    return builder.GetEdmModel();
}
