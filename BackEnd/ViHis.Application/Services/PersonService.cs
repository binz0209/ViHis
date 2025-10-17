using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ViHis.Application.DTOs;
using ViHis.Application.Utils;
using ViHis.Domain.Entities;
using ViHis.Infrastructure.Mongo;
using ViHis.Infrastructure.Repositories.Interfaces;

namespace ViHis.Application.Services;

public interface IPersonService
{
    Task<IReadOnlyList<PersonListDto>> GetAllAsync();
    Task<PersonDetailDto?> GetByIdAsync(string id);
    Task<PersonDetailDto> CreateAsync(CreatePersonDto dto);
    Task UpdateAsync(string id, UpdatePersonDto dto);
    Task DeleteAsync(string id);
}

public class PersonService : IPersonService
{
    private readonly IPersonRepository _repo;
    private readonly IMongoDbContext _ctx;

    public PersonService(IPersonRepository repo, IMongoDbContext ctx)
    {
        _repo = repo;
        _ctx = ctx;
    }

    public async Task<IReadOnlyList<PersonListDto>> GetAllAsync()
    {
        // Ép kiểu để giữ IMongoQueryable -> dùng được ToListAsync()
        var q = (IMongoQueryable<Person>)_repo.Query();

        var list = await q
            .Select(p => new PersonListDto(p.Id, p.Slug, p.FullName, p.AltName))
            .ToListAsync();

        return list.AsReadOnly();
    }

    public async Task<PersonDetailDto?> GetByIdAsync(string id)
    {
        var q = (IMongoQueryable<Person>)_repo.Query();

        var item = await q
            .Where(x => x.Id == id)
            .Select(p => new PersonDetailDto(
                p.Id,
                p.Slug,
                p.FullName,
                p.AltName,
                p.Summary,
                p.DynastyId,
                p.PeriodId))
            .FirstOrDefaultAsync();

        return item;
    }

    public async Task<PersonDetailDto> CreateAsync(CreatePersonDto dto)
    {
        var entity = new Person
        {
            Id = ObjectId.GenerateNewId().ToString(),
            FullName = dto.FullName,
            AltName = dto.AltName,
            Summary = dto.Summary,
            DynastyId = dto.DynastyId,
            PeriodId = dto.PeriodId,
            Slug = SlugHelper.ToSlug(dto.FullName),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity);

        return new PersonDetailDto(
            entity.Id,
            entity.Slug,
            entity.FullName,
            entity.AltName,
            entity.Summary,
            entity.DynastyId,
            entity.PeriodId);
    }

    public async Task UpdateAsync(string id, UpdatePersonDto dto)
    {
        var existed = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Person not found");
        existed.FullName = dto.FullName;
        existed.AltName = dto.AltName;
        existed.Summary = dto.Summary;
        existed.DynastyId = dto.DynastyId;
        existed.PeriodId = dto.PeriodId;
        existed.Slug = SlugHelper.ToSlug(dto.FullName);
        existed.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(id, existed);
    }

    public async Task DeleteAsync(string id) => await _repo.DeleteAsync(id);
}
