using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisasterReportController : ControllerBase
    {
        private readonly IDisasterReportService _reportService;

        public DisasterReportController(IDisasterReportService reportService)
        {
            _reportService = reportService;
        }
    }
}