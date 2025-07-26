using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        
        public UserRepository(AppDbContext context) { _context = context; }
        
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> GetMeAsync(Guid userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            return user;
        }

        public async Task<string?> GetUserRoleAsync(Guid userId)
        {
            // Fetch the role of the active user by userId
            var role = await _context.Users
                .Where(u => u.Id == userId && u.Status == "Active")
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            // role will be null if user not found or inactive
            return role;
        }


        // NEW: Get user by external ID for social logins
        public async Task<User?> GetByExternalIdAsync(string externalId, string authProvider)
        {
            return await _context.Users.SingleOrDefaultAsync(u =>
                u.ExternalId == externalId && u.AuthProvider == authProvider);
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(string search = null, string role = null, string status = null)
        {
            var query = _context.Users.AsQueryable();

            // Apply same filters as GetPaginatedAsync
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.Status == status);
            }

            return await query.CountAsync();
        }
        // UserRepository.cs
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // UserRepository.cs
     public async Task<IEnumerable<User>> GetPaginatedAsync(
    int skip,
    int take,
    string search = null,
    string role = null,
    string status = null)
        {
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.Status == status);
            }

            return await query
                .OrderBy(u => u.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}
