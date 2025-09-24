using DisasterManagementSystem_Data.Models;

namespace DisasterManagementSystem_Data.Repositories.Interfaces;

public interface IFinancialAllocationRepository
{
    Task<AllocationType?> GetAllocationTypeByNameAsync(string name);
    Task AddAllocationTypeAsync(AllocationType allocationType);
    Task<FinancialAllocation?> GetFinancialAllocationByIdAsync(int allocationId);
    Task AddFinancialAllocationAsync(FinancialAllocation allocation);
    Task<IEnumerable<FinancialAllocation>> GetFinancialAllocationsByYearAsync(int startYear, int endYear);
    Task SaveChangesAsync();
    Task<IEnumerable<FinancialAllocation>> GetFinancialAllocationsByYearAsync(int year);
    Task UpdateFinancialAllocationAsync(FinancialAllocation allocation);
    Task<bool> DeleteFinancialAllocationAsync(int allocationId);
    Task<(decimal? TotalDonations, decimal? TotalAllocations, decimal? Difference)> GetLastYearTotalsAsync(int year);
    Task<(decimal? TotalDonations, decimal? TotalAllocations, int TotalAllocationsCount, decimal? Difference)> GetOverviewAsync(int year);


}
