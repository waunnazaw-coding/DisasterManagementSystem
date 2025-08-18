using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories.Implements;

public class FinancialAllocationRepository : IFinancialAllocationRepository
{
    private readonly AppDbContext _context;

    public FinancialAllocationRepository(AppDbContext context)
    {
        _context = context;
    }

    
    public async Task<AllocationType?> GetAllocationTypeByNameAsync(string name)
    {
        return await _context.AllocationTypes.FirstOrDefaultAsync(a => a.Name == name);
    }

    
    public async Task AddAllocationTypeAsync(AllocationType allocationType)
    {
        await _context.AllocationTypes.AddAsync(allocationType);
    }

    
    public async Task AddFinancialAllocationAsync(FinancialAllocation allocation)
    {
        await _context.FinancialAllocations.AddAsync(allocation);
    }

    
    public async Task<FinancialAllocation?> GetFinancialAllocationByIdAsync(int allocationId)
    {
        return await _context.FinancialAllocations
            .Include(fa => fa.AllocationType)
            .FirstOrDefaultAsync(fa => fa.Id == allocationId);
    }
    
    
    public async Task<IEnumerable<FinancialAllocation>> GetFinancialAllocationsByYearAsync(int startYear, int endYear)
    {
        return await _context.FinancialAllocations
            .Include(fa => fa.AllocationType)
            .Where(fa => fa.AllocationDate.Year >= startYear && fa.AllocationDate.Year <= endYear)
            .ToListAsync();
    }
    
    
    public async Task<IEnumerable<FinancialAllocation>> GetFinancialAllocationsByYearAsync(int year)
    {
        return await _context.FinancialAllocations
            .Include(fa => fa.AllocationType)
            .Where(fa => fa.AllocationDate.Year == year)
            .ToListAsync();
    }

    
    public async Task UpdateFinancialAllocationAsync(FinancialAllocation allocation)
    {
        // Attach entity and set state to Modified to update
        _context.FinancialAllocations.Update(allocation);
        await _context.SaveChangesAsync();
    }
    
    
    public async Task<bool> DeleteFinancialAllocationAsync(int allocationId)
    {
        var allocation = await _context.FinancialAllocations.FindAsync(allocationId);
        if (allocation == null)
        {
            return false; // Not found
        }

        _context.FinancialAllocations.Remove(allocation);
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<(decimal? TotalDonations, decimal? TotalAllocations, int TotalAllocationsCount, decimal? Difference)> GetOverviewAsync(int year)
    {
        // Sum donations for the given year
        var totalDonations = await _context.Donations
            .Where(d => d.DateReceived != null && d.DateReceived.Year == year)
            .SumAsync(d => (decimal?)d.Amount);

        // Sum allocations for the given year
        var totalAllocations = await _context.FinancialAllocations
            .Where(fa => fa.AllocationDate != null && fa.AllocationDate.Year == year)
            .SumAsync(fa => (decimal?)fa.Amount);

        // Count allocations for the given year
        var totalAllocationsCount = await _context.FinancialAllocations
            .Where(fa => fa.AllocationDate != null && fa.AllocationDate.Year == year)
            .CountAsync();

        // Calculate difference
        var difference = (totalDonations ?? 0) - (totalAllocations ?? 0);

        return (totalDonations, totalAllocations, totalAllocationsCount, difference);
    }





    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<(decimal? TotalDonations, decimal? TotalAllocations, decimal? Difference)> GetLastYearTotalsAsync(int year)
    {
        // Use the passed "year" parameter instead of calculating last year
        var totalDonations = await _context.Donations
            .Where(d => d.DateReceived != null && d.DateReceived.Year == year)
            .SumAsync(d => (decimal?)d.Amount);

        var totalAllocations = await _context.FinancialAllocations
            .Where(fa => fa.AllocationDate != null && fa.AllocationDate.Year == year)
            .SumAsync(fa => (decimal?)fa.Amount);

        var difference = (totalDonations ?? 0) - (totalAllocations ?? 0);

        return (totalDonations, totalAllocations, difference);
    }

}
