using VietHistory.Application.DTOs;
using VietHistory.Application.Services;
using VietHistory.Domain.Common;
using VietHistory.Domain.Entities;
using VietHistory.Infrastructure.Mongo;

namespace VietHistory.Infrastructure.Services;

public class PeopleService : IPeopleService
{
    private readonly IRepository<Person> _repo;
    public PeopleService(MongoContext ctx)
    {
        _repo = new MongoRepository<Person>(ctx.People);
    }

    public async Task<PersonDto> CreateAsync(CreatePersonRequest req)
    {
        var p = new Person { FullName = req.FullName, BornYear = req.BornYear, DiedYear = req.DiedYear, Summary = req.Summary, Slug = Slugify(req.FullName) };
        await _repo.AddAsync(p);
        return new PersonDto(p.Id, p.FullName, p.BornYear, p.DiedYear, p.Summary);
    }

    public async Task<IReadOnlyList<PersonDto>> ListAsync(string? q = null, int skip = 0, int take = 50)
    {
        q = (q ?? "").Trim();
        IReadOnlyList<Person> list;
        if (string.IsNullOrWhiteSpace(q))
            list = await _repo.FindAsync(x => true, skip, take);
        else
            list = await _repo.FindAsync(x => x.FullName.ToLower().Contains(q.ToLower()) || (x.Summary ?? "").ToLower().Contains(q.ToLower()), skip, take);

        return list.Select(x => new PersonDto(x.Id, x.FullName, x.BornYear, x.DiedYear, x.Summary)).ToList();
    }

    private static string Slugify(string input)
    {
        var slug = input.ToLowerInvariant();
        slug = slug.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in slug)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        slug = new string(sb.ToString().Where(ch => char.IsLetterOrDigit(ch) || ch == ' ').ToArray()).Trim().Replace(' ', '-');
        return slug;
    }
}

public class EventsService : IEventsService
{
    private readonly IRepository<Event> _repo;

    public EventsService(MongoContext ctx)
    {
        _repo = new MongoRepository<Event>(ctx.Events);
    }

    public async Task<EventDto> CreateAsync(CreateEventRequest req)
    {
        var e = new Event { Title = req.Title, Year = req.Year, Month = req.Month, Day = req.Day, Summary = req.Summary, Slug = Slugify(req.Title) };
        await _repo.AddAsync(e);
        return new EventDto(e.Id, e.Title, e.Year, e.Month, e.Day, e.Summary);
    }

    public async Task<IReadOnlyList<EventDto>> ListAsync(int? month = null, int? day = null, string? q = null, int skip = 0, int take = 50)
    {
        q = (q ?? "").Trim();

        IReadOnlyList<Event> list = await _repo.FindAsync(x =>
            (month == null || x.Month == month) &&
            (day == null || x.Day == day) &&
            (string.IsNullOrWhiteSpace(q) || (x.Title.ToLower().Contains(q.ToLower()) || (x.Summary ?? "").ToLower().Contains(q.ToLower()))),
            skip, take);

        return list.Select(x => new EventDto(x.Id, x.Title, x.Year, x.Month, x.Day, x.Summary)).ToList();
    }

    private static string Slugify(string input)
    {
        var slug = input.ToLowerInvariant();
        slug = slug.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in slug)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        slug = new string(sb.ToString().Where(ch => char.IsLetterOrDigit(ch) || ch == ' ').ToArray()).Trim().Replace(' ', '-');
        return slug;
    }
}
