using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.DisasterTypsDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DisasterTypeService : IDisasterTypeService
{
    private readonly IDisasterTypeRepository _repository;

    public DisasterTypeService(IDisasterTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DisasterType>> GetByIdAsync(int id)
    {
        var disasterType = await _repository.GetByIdAsync(id);
        return disasterType != null
             ? Result<DisasterType>.Success(disasterType)
             : Result<DisasterType>.Failure($"DisasterType with ID {id} not found.");
    }

    public async Task<Result<IEnumerable<DisasterType>>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return list != null
            ? Result<IEnumerable<DisasterType>>.Success(list)
            : Result<IEnumerable<DisasterType>>.Failure("No disaster types available.");
    }

    public async Task<Result<DisasterType>> AddAsync(DisasterTypeCreateDto dto)
    {
        if (!IsValidCategory(dto.Category))
            return Result<DisasterType>.ValidationError("Invalid category for disaster type.");

        var entity = new DisasterType
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category
        };

        await _repository.AddAsync(entity);

        return Result<DisasterType>.Success(entity, "Disaster type created successfully.");
    }

    public async Task<Result<DisasterType>> UpdateAsync(DisasterTypeUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.Id);
        if (existing == null)
            return Result<DisasterType>.NotFoundError($"DisasterType with ID {dto.Id} not found.");

        if (!IsValidCategory(dto.Category))
            return Result<DisasterType>.ValidationError("Invalid category. Must be 'Natural Disaster' or 'Artificial Disaster'.");

        // Apply changes from DTO to entity
        existing.Name = dto.Name;
        existing.Category = dto.Category;
        existing.Description = dto.Description;

        await _repository.UpdateAsync(existing);
        await _repository.SaveChangesAsync();

        return Result<DisasterType>.Success(existing, "Disaster type updated successfully.");
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return Result<bool>.NotFoundError($"DisasterType with ID {id} not found.");

        await _repository.DeleteAsync(id);

        return Result<bool>.Success(true, "Deleted successfully.");
    }

    

    private bool IsValidCategory(string category)
    {
        return category == "Natural Disaster" || category == "Artificial Disaster";
    }
}
