using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            _context.ReportPhotos.Add(photo);
            await _context.SaveChangesAsync();
            return photo;
        }
    }
}
