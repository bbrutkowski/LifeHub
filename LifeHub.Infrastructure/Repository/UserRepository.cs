using LifeHub.Application.Interfaces;
using LifeHub.Database;
using LifeHub.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace LifeHub.Infrastructure.Repository
{
    public class UserRepository(ApplicationDbContext db) : IUserRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task Add(User user, CancellationToken cancellationToken = default)
        {
            await _db.Users.AddAsync(user, cancellationToken);
            await SaveChanges(cancellationToken);
        }

        public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task Update(User user, CancellationToken cancellationToken = default)
        {
            _db.Users.Update(user);
            await SaveChanges(cancellationToken);
        }

        private async Task SaveChanges(CancellationToken cancellationToken = default)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
