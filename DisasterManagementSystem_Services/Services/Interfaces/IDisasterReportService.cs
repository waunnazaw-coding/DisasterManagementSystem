using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;

public interface IDisasterReportService
{
    Task<Result<DisasterReport>> GetByIdAsync(int id);
    Task<Result<IEnumerable<DisasterReport>>> GetAllAsync();
    Task<Result<FormCreateDto>> AddFormAsync(FormCreateDto dto);
    Task<Result<FormUpdateDto>> UpdateFormAsync(FormUpdateDto dto);
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<bool>> ApproveAsync(int id);
    Task<Result<bool>> DisapproveAsync(int id);
}