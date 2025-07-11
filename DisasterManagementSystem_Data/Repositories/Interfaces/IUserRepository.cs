using DisasterManagementSystem_Data.Models;

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
        Task<User> GetMeAsync(Guid userId);
        // New method for social login
        Task<User?> GetByExternalIdAsync(string externalId, string authProvider);
    }
}
