using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;

public interface IDisasterReportService
{
    Task<Result<DisasterReportDetailsDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<DisasterReport>>> GetAllAsync();
    Task<Result<FormCreateDto>> AddFormAsync(FormCreateDto dto);
    Task<Result<FormUpdateDto>> UpdateFormAsync(FormUpdateDto dto);
    Task<Result<List<DisasterReport>>> GetWillDeleteRemindersAsync();
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<bool>> ApproveAsync(int reportId);
    Task<Result<bool>> DisapproveAsync(int id);
    Task<Result<bool>> UnrejectReportAsync(int reportId);
    Task<Result<bool>> MarkAsCheckedAsync(int reportId);
    Task<Result<bool>> MarkAsFakeAsync(int reportId);
    Task<Result<ReportImpactCreateDto>> CreateAsync(ReportImpactCreateDto dto);
}