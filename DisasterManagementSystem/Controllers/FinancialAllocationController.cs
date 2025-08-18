using DisasterManagementSystem_Services.Models.FinancialAllocationDtos;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace DisasterManagementSystem_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialAllocationsController : ControllerBase
    {
        private readonly IFinancialAllocationService _financialAllocationService;
        private readonly IUserContextService _userContextService;

        public FinancialAllocationsController(IFinancialAllocationService financialAllocationService , IUserContextService userContextService)
        {
            _financialAllocationService = financialAllocationService;
            _userContextService = userContextService;
        }

        [HttpPost]
        public async Task<IResult> Create([FromBody] FinancialAllocationRequestDto dto)
        {
            if (dto == null)
                return Results.BadRequest(Result<FinancialAllocationResponseDto>.ValidationError("Invalid request data."));

            var currentUserId = _userContextService.GetCurrentUserId();
            if (currentUserId is null)
            {
                throw new Exception("Current user ID not found.");
            }

            var result = await _financialAllocationService.CreateAsync(dto , currentUserId);
            return result.Execute();
        }


        [HttpGet("reports")]
        public async Task<IResult> GetAnnualReport([FromQuery] int startYear, [FromQuery] int endYear)
        {
            var result = await _financialAllocationService.GetAnnualReportAsync(startYear, endYear);
            return result.Execute();
        }



        [HttpGet("annual-reports/{year}")]
        public async Task<IResult> GetFinancialAllocationsByYearAsync( int year)
        {
            var result = await _financialAllocationService.GetFinancialAllocationsByYearAsync(year);
            return result.Execute();
        }



        [HttpGet("totals/{year}")]
        public async Task<IActionResult> GetTotals(int year)
        {
            var result = await _financialAllocationService.GetOverviewAsync(year);

            if (result.TotalDonations == null && result.TotalAllocations == null)
                return NotFound("No data found for the specified year.");

            return Ok(new
            {
                TotalDonations = result.TotalDonations,
                TotalAllocations = result.TotalAllocations,
                TotalAllocationsCount = result.TotalAllocationsCount,
                Difference = result.Difference
            });
        }


        [HttpPut("{id}")]
        public async Task<IResult> Update(int id, [FromBody] FinancialAllocationRequestDto dto)
        {
            if (dto == null)
                return Results.BadRequest(Result<bool>.ValidationError("Invalid request data."));

            var result = await _financialAllocationService.UpdateAsync(id, dto);
            return result.Execute();
        }


        [HttpDelete("{id}")]
        public async Task<IResult> Delete(int id)
        {
            var result = await _financialAllocationService.DeleteAsync(id);
            return result.Execute();
        }


        [HttpPost("import-excel")]
        public async Task<IResult> ImportFromExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest(Result<string>.ValidationError("No file uploaded."));

            try
            {
                using var stream = file.OpenReadStream();
                await _financialAllocationService.ImportFromExcelAsync(stream);
                return Results.Ok(Result<string>.Success(null, "Excel import successful."));
            }
            catch (System.Exception ex)
            {
                var errorResult = Result<string>.Failure($"Excel import failed: {ex.Message}");
                return errorResult.Execute();
            }

        }


        [HttpGet("annual-report/pdf")]
        public async Task<IResult> DownloadAnnualReportPdf([FromQuery] int year)
        {

            try
            {
                var pdfBytes = await _financialAllocationService.GenerateAnnualReportPdfAsync(year);
                return Results.File(pdfBytes, "application/pdf", $"FinancialReport_{year}.pdf");
            }
            catch (System.Exception ex)
            {
                var errorResult = Result<string>.Failure($"PDF generate failed: {ex.Message}");
                return errorResult.Execute();
            }
        }


        [HttpGet("percentages/{year}")]
        public async Task<ActionResult<List<AllocationTypeSummary>>> GetAllocationPercentages(int year)
        {
            var result = await _financialAllocationService.GetAllocationTypePercentagesAsync(year);

            if (result == null || result.Count == 0)
                return NotFound($"No allocations found for year {year}.");

            return Ok(result);
        }
    }
}
