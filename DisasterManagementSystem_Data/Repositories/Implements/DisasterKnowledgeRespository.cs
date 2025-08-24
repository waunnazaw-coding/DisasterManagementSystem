using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories.Implements;

public class DisasterKnowledgeRepository : IDisasterKnowledgeRepository
{
    private readonly AppDbContext _context;
    public DisasterKnowledgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DisasterKnowledge> AddAsync(DisasterKnowledge entity)
    {
        entity.DateCreated = DateTime.UtcNow;
        entity.DateUpdated = DateTime.UtcNow;
        _context.DisasterKnowledges.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.DisasterKnowledges.FindAsync(id);
        if (entity == null) return false;
        _context.DisasterKnowledges.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DisasterKnowledge>> GetAllAsync()
    {
        return await _context.DisasterKnowledges.Include(d => d.Resources).ToListAsync();
    }

    public async Task<DisasterKnowledge?> GetByIdAsync(int id)
    {
        return await _context.DisasterKnowledges.Include(d => d.Resources).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DisasterKnowledge?> UpdateAsync(DisasterKnowledge entity)
    {
        var existing = await _context.DisasterKnowledges.FindAsync(entity.Id);
        if (existing == null) return null;
        existing.Title = entity.Title;
        existing.ContentType = entity.ContentType;
        existing.DisasterType = entity.DisasterType;
        existing.AuthorId = entity.AuthorId;
        existing.Content = entity.Content;
        existing.Language = entity.Language;
        existing.ReferenceFrom = entity.ReferenceFrom;
        existing.DateUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return existing;
    }
}