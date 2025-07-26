using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.Pagination;
using DisasterManagementSystem_Services.Models.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserDto>> GetByIdAsync(Guid id);
        Task<Result<PaginatedResult<UserDto>>> GetPaginatedAsync(int pageNumber, int pageSize,string search,string role,string status);
        Task<Result<UserDto>> CreateAsync(UserCreateDto dto);
        Task<Result<UserDto>> UpdateAsync(UserUpdateDto dto);
        Task<Result<bool>> DeleteAsync(Guid id);
        Task<Result<UserDto>> BlockUserAsync(Guid id);
        Task<Result<UserDto>> UnblockUserAsync(Guid id);
        Task<Result<UserDto>> ChangeUserRoleAsync(Guid id, string role);

    }
}
