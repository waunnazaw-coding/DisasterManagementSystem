using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        
        
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Attach(User entity)
        {
            _context.Attach(entity);
        }

        public EntityEntry<User> Entry(User entity)
        {
            return _context.Entry(entity);
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
    }
}
