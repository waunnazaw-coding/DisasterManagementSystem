// ImpactRepository.cs
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data; // Assuming your DbContext namespace
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class ImpactRepository : IImpactRepository
{
    private readonly AppDbContext _context;
    public ImpactRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<Impact> impacts)
    {
        await _context.Impacts.AddRangeAsync(impacts);
        await _context.SaveChangesAsync();
    }
}
