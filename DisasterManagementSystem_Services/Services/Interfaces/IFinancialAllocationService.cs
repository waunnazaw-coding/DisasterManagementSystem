using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.FinancialAllocationDtos;

namespace DisasterManagementSystem_Services.Services.Interfaces;

public interface IFinancialAllocationService
{
    Task<Result<FinancialAllocationResponseDto>> CreateAsync(FinancialAllocationRequestDto dto, Guid? currentUserId);

    Task<Result<FinancialAllocationResponseDto?>> GetByIdAsync(int allocationId);

    Task<Result<IEnumerable<FinancialAllocationResponseDto>>> GetAnnualReportAsync(int startYear, int endYear);
    Task<Result<IEnumerable<FinancialAllocationResponseDto>>> GetFinancialAllocationsByYearAsync(int year);

    Task<Result<FinancialAllocationResponseDto>> UpdateAsync(int allocationId, FinancialAllocationRequestDto dto);

    Task<Result<bool>> DeleteAsync(int allocationId);

    Task ImportFromExcelAsync(Stream excelStream);

    Task<byte[]> GenerateAnnualReportPdfAsync(int year);

    Task<List<AllocationTypeSummary>> GetAllocationTypePercentagesAsync(int year);

    Task<(decimal? TotalDonations, decimal? TotalAllocations, int TotalAllocationsCount, decimal? Difference)> GetOverviewAsync(int year);
}