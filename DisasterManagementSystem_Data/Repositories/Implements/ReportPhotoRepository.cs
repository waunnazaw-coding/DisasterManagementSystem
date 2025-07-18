using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Implements
{
    public class ReportPhotoRepository : IReportPhotoRepository
    {
        private readonly AppDbContext _context;

        public ReportPhotoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReportPhoto> AddAsync(ReportPhoto photo)
        {
            await _context.ReportPhotos.AddAsync(photo);
            return photo;
        }

        public async Task<ReportPhoto?> UpdateAsync(ReportPhoto photo)
        {
            var existingPhoto = await _context.ReportPhotos.FindAsync(photo.Id);
            if (existingPhoto == null) return null;

            existingPhoto.FilePath = photo.FilePath;
            existingPhoto.FileType = photo.FileType;
            existingPhoto.FileSize = photo.FileSize;
            existingPhoto.UploadedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingPhoto;
        }

        public async Task<List<ReportPhoto>> GetByReportIdAsync(int reportId)
        {
            return await _context.ReportPhotos
                .Where(p => p.DisasterReportId == reportId)
                .ToListAsync();
        }

        public async Task<List<ReportPhoto>> GetByEventIdAsync(int eventId)
        {
            return await _context.ReportPhotos
                .Where(p => p.DisasterEventId == eventId)
                .ToListAsync();
        }

        public async Task<ReportPhoto?> GetByIdAsync(int photoId)
        {
            return await _context.ReportPhotos.FindAsync(photoId);
        }

        public async Task<bool> DeleteAsync(int photoId)
        {
            var photo = await _context.ReportPhotos.FindAsync(photoId);
            if (photo == null) return false;

            _context.ReportPhotos.Remove(photo);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}