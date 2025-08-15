using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories.Implements;


public class ResourceRepository : IResourceRepository
{
    private readonly AppDbContext _context;

    public ResourceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Resource> AddAsync(Resource entity)
    {
        entity.DateAdded = DateTime.UtcNow;
        _context.Resources.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Resource?> UpdateAsync(Resource entity)
    {
        var existingResource = await _context.Resources.FindAsync(entity.Id);
        if (existingResource == null)
            return null;

        // Update fields (except Id and DisasterKnowledgeId which should remain unchanged)
        existingResource.ResourceType = entity.ResourceType;
        existingResource.Url = entity.Url;
        existingResource.Description = entity.Description;
        // Optionally update DateAdded if you want to track updated timestamp separately or add a DateUpdated property if you have it
        // existingResource.DateAdded = entity.DateAdded;

        await _context.SaveChangesAsync();
        return existingResource;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Resources.FindAsync(id);
        if (entity == null) return false;
        _context.Resources.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Resource>> GetByDisasterKnowledgeIdAsync(int disasterKnowledgeId)
    {
        return await _context.Resources.Where(r => r.DisasterKnowledgeId == disasterKnowledgeId).ToListAsync();
    }

    public async Task<Resource?> GetByIdAsync(int id)
    {
        return await _context.Resources.FindAsync(id);
    }
}