using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.Pagination;
using DisasterManagementSystem_Services.Models.UserDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<PaginatedResult<UserDto>>> GetPaginatedAsync(
         int pageNumber,
         int pageSize,
         string search = null,
         string role = null,
         string status = null)
        {
            if (pageNumber < 1 || pageSize < 1)
                return Result<PaginatedResult<UserDto>>.ValidationError("Page number and size must be positive integers");

            var skip = (pageNumber - 1) * pageSize;

            // Get filtered and paginated users
            var users = await _userRepository.GetPaginatedAsync(
                skip,
                pageSize,
                search,
                role,
                status);

            // Get total count with same filters
            var totalRecords = await _userRepository.CountAsync(
                search,
                role,
                status);

            var dtos = users.Select(MapToDto).ToList();

            var result = new PaginatedResult<UserDto>(
                dtos,
                pageNumber,
                pageSize,
                totalRecords
            );

            return Result<PaginatedResult<UserDto>>.Success(result);
        }

        // Other methods remain the same as in your original implementation
        public async Task<Result<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return Result<UserDto>.NotFoundError("User not found");

            return Result<UserDto>.Success(MapToDto(user));
        }

        public async Task<Result<UserDto>> CreateAsync(UserCreateDto dto)
        {
            if (await _userRepository.GetByEmailAsync(dto.Email) != null)
                return Result<UserDto>.ValidationError("Email already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                Status = "Active",
                AuthProvider = dto.AuthProvider,
                ExternalId = dto.ExternalId,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.AuthProvider == null && !string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = HashPassword(dto.Password); // Implement your hashing
            }

            await _userRepository.AddAsync(user);
            return Result<UserDto>.Success(MapToDto(user), "User created successfully");
        }

        public async Task<Result<UserDto>> UpdateAsync(UserUpdateDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null)
                return Result<UserDto>.NotFoundError("User not found");

            if (!string.IsNullOrEmpty(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Email))
            {
                if (await _userRepository.GetByEmailAsync(dto.Email) != null)
                    return Result<UserDto>.ValidationError("Email already in use");
                user.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Role))
                user.Role = dto.Role;

            if (!string.IsNullOrEmpty(dto.Status))
                user.Status = dto.Status;

            await _userRepository.UpdateAsync(user);
            return Result<UserDto>.Success(MapToDto(user), "User updated successfully");
        }

        // ... existing code ...

        // In UserService.cs
        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return Result<bool>.NotFoundError("User not found");

            // First delete all related records
            await _userRepository.DeleteUserRelatedRecordsAsync(id);

            // Then delete the user
            await _userRepository.DeleteAsync(user);
            return Result<bool>.Success(true, "User deleted successfully");
        }
        // ... rest of the code ...

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                AuthProvider = user.AuthProvider,
                CreatedAt = user.CreatedAt
            };
        }

        private string HashPassword(string password)
        {
            // Implement your password hashing logic here
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public async Task<Result<UserDto>> BlockUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return Result<UserDto>.NotFoundError("User not found");

            user.Status = "Blacklisted";
            await _userRepository.UpdateAsync(user);

            return Result<UserDto>.Success(MapToDto(user), "User blocked successfully");
        }

        public async Task<Result<UserDto>> UnblockUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return Result<UserDto>.NotFoundError("User not found");

            user.Status = "Active";
            await _userRepository.UpdateAsync(user);

            return Result<UserDto>.Success(MapToDto(user), "User unblocked successfully");
        }

        public async Task<Result<UserDto>> ChangeUserRoleAsync(Guid id, string role)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return Result<UserDto>.NotFoundError("User not found");

            user.Role = role;
            await _userRepository.UpdateAsync(user);

            return Result<UserDto>.Success(MapToDto(user), "User role updated successfully");
        }

    }
}
