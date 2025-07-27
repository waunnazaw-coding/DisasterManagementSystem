// IImpactRepository.cs
using DisasterManagementSystem_Data.Models;
using System.Threading.Tasks;

public interface IImpactRepository
{
    Task AddRangeAsync(IEnumerable<Impact> impacts);
}
