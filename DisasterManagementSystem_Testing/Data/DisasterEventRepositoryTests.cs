using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;
namespace DisasterManagementSystem_Testing.Data
{
    public class DisasterEventRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DisasterEventRepository _repository;
        public DisasterEventRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new DisasterEventRepository(_context);

            SeedData();
        }
        private void SeedData()
        {
            var event1 = new DisasterEvent
            {
                Id = 1,
                Name = "Flood 2025",
                Status = "Active",
                DisasterType = new DisasterType { Id = 1, Name = "Flood" },
                Location = new Location { Id = 1, Name = "River Town" }
            };

            var event2 = new DisasterEvent
            {
                Id = 2,
                Name = "Earthquake 2025",
                Status = "Closed",
                DisasterType = new DisasterType { Id = 2, Name = "Earthquake" },
                Location = new Location { Id = 2, Name = "Mountain City" }
            };

            _context.DisasterEvents.AddRange(event1, event2);
            _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}