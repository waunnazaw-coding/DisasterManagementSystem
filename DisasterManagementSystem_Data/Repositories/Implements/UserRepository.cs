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
        public async Task<List<User>> GetUsersByRoleAsync(string roles)
        {
            if (string.IsNullOrEmpty(roles))
                return new List<User>();

            var roleList = roles.Split(',').Select(r => r.Trim().ToLower()).ToList();

            var users = await _context.Users
                .Where(u => u.Role != null &&
                            roleList.Any(role => EF.Functions.Like(u.Role, role + "%")))
                .ToListAsync();

            return users;
        }


        // Add this method implementation
        public async Task DeleteUserNotificationsAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
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

        // In UserRepository.cs
        public async Task DeleteUserRelatedRecordsAsync(Guid userId)
        {
            // 1. First delete RequestAssignment records that might reference AssistanceRequests
            var requestAssignments = await _context.RequestAssignments
                .Where(ra => _context.AssistanceRequests
                    .Where(ar => ar.UserId == userId)
                    .Select(ar => ar.Id)
                    .Contains(ra.AssistanceRequestId))
                .ToListAsync();
            _context.RequestAssignments.RemoveRange(requestAssignments);

            // Delete notifications
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);

            // Delete donations
            var donations = await _context.Donations
                .Where(d => d.DonorUserId == userId)
                .ToListAsync();
            _context.Donations.RemoveRange(donations);

            var requests = await _context.AssistanceRequests.
                Where(r => r.UserId == userId).ToListAsync();
            _context.AssistanceRequests.RemoveRange(requests);

            var reports = await _context.DisasterReports.Where(report => report.UserId == userId).ToListAsync();
            


            await _context.SaveChangesAsync();
        }

        // Fetch emails of all admin users
        public async Task<List<string>> GetAdminEmailsAsync()
        {
            return await _context.Users
                .Where(user => user.Role == "Admin")    
                .Select(user => user.Email)
                .Where(email => !string.IsNullOrEmpty(email))
                .ToListAsync();
        }
    }
}
