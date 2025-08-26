using DisasterManagementSystem_Data.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<string?> GetUserRoleAsync(Guid userId);
        Task<User> GetMeAsync(Guid userId);
        void Attach(User entity);
        EntityEntry<User> Entry(User entity);
        Task SaveChangesAsync();

        Task<User?> GetByExternalIdAsync(string externalId, string authProvider);
       Task<List<User>> GetUsersByRoleAsync(string role);

        Task DeleteAsync(User user);
        Task<int> CountAsync(string search = null, string role = null, string status = null);
        Task<IEnumerable<User>> GetAllAsync(); // Add this line

        // IUserRepository.cs
        Task<IEnumerable<User>> GetPaginatedAsync(int skip, int take, string search = null, string role = null, string status = null);

        Task<List<string>> GetAdminEmailsAsync();
    }
}
