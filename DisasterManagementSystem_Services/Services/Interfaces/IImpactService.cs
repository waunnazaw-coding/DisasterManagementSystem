public interface IImpactService
{
    Task<bool> CreateImpactsAsync(IEnumerable<ImpactCreateDto> dtos);
}