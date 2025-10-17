
using MongoDB.Driver;
using ViHis.Domain.Entities;
using ViHis.Infrastructure.Mongo;

namespace ViHis.Infrastructure.Repositories;
public class PersonRepository : GenericRepository<Person>, ViHis.Infrastructure.Repositories.Interfaces.IPersonRepository
{
    public PersonRepository(IMongoDbContext ctx) : base(ctx.People) {}
}
