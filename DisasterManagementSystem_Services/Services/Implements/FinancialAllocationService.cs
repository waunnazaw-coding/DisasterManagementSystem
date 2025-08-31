using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models.FinancialAllocationDtos;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using QuestPDF.Fluent;
using ScottPlot;
using QuestPDF.Helpers;
using Colors = QuestPDF.Helpers.Colors;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class FinancialAllocationService : IFinancialAllocationService
    {
        private readonly IFinancialAllocationRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly AppDbContext _context;

        public FinancialAllocationService(IFinancialAllocationRepository repository, AppDbContext context , IUserContextService userContextService)
        {
            _repository = repository;
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<Result<FinancialAllocationResponseDto>> CreateAsync(FinancialAllocationRequestDto dto, Guid? currentUserId)
        {
            try
            {
                var allocationType = await _repository.GetAllocationTypeByNameAsync(dto.AllocationTypeName);
                if (allocationType == null)
                {
                    allocationType = new AllocationType { Name = dto.AllocationTypeName };
                    await _repository.AddAllocationTypeAsync(allocationType);
                    await _repository.SaveChangesAsync();
                }

                var entity = new FinancialAllocation
                {
                    AllocationTypeId = allocationType.AllocationTypeId,
                    Amount = dto.Amount,
                    AllocationDate = dto.AllocationDate,
                    CreatedBy = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    Notes = dto.Notes,
                    DetailName = dto.DetailName,
                    DetailDescription = dto.DetailDescription
                };

                await _repository.AddFinancialAllocationAsync(entity);
                await _repository.SaveChangesAsync();

                var responseDto = new FinancialAllocationResponseDto
                {
                    AllocationId = entity.Id,
                    //DonationId = entity.DonationId,
                    AllocationTypeName = allocationType.Name,
                    Amount = entity.Amount,
                    AllocationDate = entity.AllocationDate,
                    CreatedBy = entity.CreatedBy,
                    Notes = entity.Notes,
                    DetailName = entity.DetailName,
                    DetailDescription = entity.DetailDescription
                };

                return Result<FinancialAllocationResponseDto>.Success(responseDto);
            }
            catch (Exception ex)
            {
                return Result<FinancialAllocationResponseDto>.Failure($"An error occurred during creation: {ex.Message}");
            }
        }


        public async Task<Result<FinancialAllocationResponseDto?>> GetByIdAsync(int allocationId)
        {
            try
            {
                var entity = await _repository.GetFinancialAllocationByIdAsync(allocationId);
                if (entity == null)
                    return Result<FinancialAllocationResponseDto?>.NotFoundError();

                var dto = MapToResponseDto(entity, entity.AllocationType?.Name ?? string.Empty);
                return Result<FinancialAllocationResponseDto?>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<FinancialAllocationResponseDto?>.Failure($"An error occurred when fetching allocation: {ex.Message}");
            }
        }

        public async Task<(decimal? TotalDonations, decimal? TotalAllocations, decimal? Difference)> GetLastYearTotalsAsync(int year)
        {
            
            return await _repository.GetLastYearTotalsAsync(year);

        }


        public async Task<List<AllocationTypeSummary>> GetAllocationTypePercentagesAsync(int year)
        {
            var allocationsInYear = _context.FinancialAllocations
                .Where(fa => fa.AllocationDate.Year == year);

            var totalAmountForYear = await allocationsInYear.SumAsync(fa => fa.Amount);

            // Guard against division by zero if no allocations
            if (totalAmountForYear == 0)
                return new List<AllocationTypeSummary>();

            var query = allocationsInYear
                .GroupBy(fa => fa.AllocationType)
                .Select(g => new AllocationTypeSummary
                {
                    AllocationTypeName = g.Key.Name,
                    TotalAmount = g.Sum(fa => fa.Amount),
                    PercentageOfYear = Math.Round((g.Sum(fa => fa.Amount) / totalAmountForYear) * 100)
                })
                .OrderByDescending(x => x.PercentageOfYear);

            return await query.ToListAsync();
        }


        public async Task<Result<IEnumerable<FinancialAllocationResponseDto>>> GetAnnualReportAsync(int startYear, int endYear)
        {
            try
            {
                if (startYear > endYear)
                    return Result<IEnumerable<FinancialAllocationResponseDto>>.ValidationError("startYear must be less than or equal to endYear.");

                var allocations = await _repository.GetFinancialAllocationsByYearAsync(startYear, endYear);
                var report = allocations.Select(fa => new FinancialAllocationResponseDto
                {
                    AllocationId = fa.Id,
                    //DonationId = fa.DonationId,
                    AllocationTypeName = fa.AllocationType.Name,
                    Amount = fa.Amount,
                    AllocationDate = fa.AllocationDate,
                    CreatedBy = fa.CreatedBy,
                    Notes = fa.Notes,
                    DetailName = fa.DetailName,
                    DetailDescription = fa.DetailDescription
                });

                return Result<IEnumerable<FinancialAllocationResponseDto>>.Success(report);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<FinancialAllocationResponseDto>>.Failure($"Failed to get annual report: {ex.Message}");
            }
        }


        public async Task<Result<IEnumerable<FinancialAllocationResponseDto>>> GetFinancialAllocationsByYearAsync(int year)
        {
            try
            {
                var nowYear = DateTime.Now.Year;

                if (year > nowYear)
                    return Result<IEnumerable<FinancialAllocationResponseDto>>.ValidationError("startYear must be less than or equal to endYear.");

                var allocations = await _repository.GetFinancialAllocationsByYearAsync(year);
                var report = allocations.Select(fa => new FinancialAllocationResponseDto
                {
                    AllocationId = fa.Id,
                   // DonationId = fa.DonationId,
                    AllocationTypeName = fa.AllocationType.Name,
                    Amount = fa.Amount,
                    AllocationDate = fa.AllocationDate,
                    CreatedBy = fa.CreatedBy,
                    Notes = fa.Notes,
                    DetailName = fa.DetailName,
                    DetailDescription = fa.DetailDescription
                });

                return Result<IEnumerable<FinancialAllocationResponseDto>>.Success(report);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<FinancialAllocationResponseDto>>.Failure($"Failed to get annual report: {ex.Message}");
            }
        }


        public async Task<Result<FinancialAllocationResponseDto>> UpdateAsync(int allocationId, FinancialAllocationRequestDto dto)
        {
            try
            {
                var currentUserId = _userContextService.GetCurrentUserId();
                if (currentUserId == null)
                {
                    // Handle case where user ID is not available or invalid
                    return Result<FinancialAllocationResponseDto>.Failure("User ID is invalid or not found.");
                }

                var entity = await _repository.GetFinancialAllocationByIdAsync(allocationId);
                if (entity == null)
                    return Result<FinancialAllocationResponseDto>.NotFoundError();

                var allocationType = await _repository.GetAllocationTypeByNameAsync(dto.AllocationTypeName);
                if (allocationType == null)
                {
                    allocationType = new AllocationType { Name = dto.AllocationTypeName };
                    await _repository.AddAllocationTypeAsync(allocationType);
                    await _repository.SaveChangesAsync();
                }

                entity.AllocationTypeId = allocationType.AllocationTypeId;
                //entity.DonationId = dto.DonationId;
                entity.Amount = dto.Amount;
                entity.AllocationDate = dto.AllocationDate;
                entity.CreatedBy = currentUserId;
                entity.Notes = dto.Notes;
                entity.DetailName = dto.DetailName;
                entity.DetailDescription = dto.DetailDescription;

                await _repository.UpdateFinancialAllocationAsync(entity);

                var responseDto = new FinancialAllocationResponseDto
                {
                    AllocationId = entity.Id,
                    //DonationId = entity.DonationId,
                    AllocationTypeName = allocationType.Name,
                    Amount = entity.Amount,
                    AllocationDate = entity.AllocationDate,
                    CreatedBy = entity.CreatedBy,
                    Notes = entity.Notes,
                    DetailName = entity.DetailName,
                    DetailDescription = entity.DetailDescription
                };

                return Result<FinancialAllocationResponseDto>.Success(responseDto);
            }
            catch (Exception ex)
            {
                return Result<FinancialAllocationResponseDto>.Failure($"An error occurred during update: {ex.Message}");
            }
        }


        public async Task<Result<bool>> DeleteAsync(int allocationId)
        {
            try
            {
                var deleted = await _repository.DeleteFinancialAllocationAsync(allocationId);
                if (!deleted)
                    return Result<bool>.NotFoundError();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred during deletion: {ex.Message}");
            }
        }


        // ImportFromExcelAsync with try-catch (throws exceptions on error)
        public async Task ImportFromExcelAsync(Stream excelStream)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Waunna Zaw");

            var currentUserId = _userContextService.GetCurrentUserId();
            if (currentUserId == null)
                throw new Exception("Current user ID not found.");

            try
            {
                using var package = new ExcelPackage(excelStream);
                var worksheet = package.Workbook.Worksheets.First();

                int row = 2; // Assuming header at row 1

                while (worksheet.Cells[row, 1].Value != null) // Check column A for data presence
                {
                    var allocationTypeName = worksheet.Cells[row, 1].GetValue<string>()?.Trim();
                    var amountValue = worksheet.Cells[row, 2].Value;
                    decimal amount;

                    if (string.IsNullOrWhiteSpace(allocationTypeName))
                        throw new ArgumentException($"AllocationTypeName is missing or empty at row {row}");

                    if (amountValue == null || !decimal.TryParse(amountValue.ToString(), out amount) || amount <= 0)
                        throw new ArgumentException($"Invalid Amount at row {row}");

                    var detailName = worksheet.Cells[row, 3].GetValue<string>();
                    var detailDescription = worksheet.Cells[row, 4].GetValue<string>();
                    var notes = worksheet.Cells[row, 5].GetValue<string>();

                    object dateValue = worksheet.Cells[row, 6].Value;
                    DateTime allocationDate;
                    if (dateValue is double oaDateNumber)
                    {
                        allocationDate = DateTime.FromOADate(oaDateNumber);
                    }
                    else if (!DateTime.TryParse(dateValue.ToString(), out allocationDate))
                    {
                        throw new ArgumentException($"Invalid AllocationDate at row {row}");
                    }



                    var dto = new FinancialAllocationRequestDto
                    {
                        AllocationTypeName = allocationTypeName,
                        Amount = amount,
                        DetailName = detailName,
                        DetailDescription = detailDescription,
                        Notes = notes,
                        AllocationDate = allocationDate
                    };

                    await CreateAsync(dto, currentUserId.Value);

                    row++;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to import Excel data.", ex);
            }
        }




        // GenerateAnnualReportPdfAsync with try-catch (throws exception on error)
        public async Task<byte[]> GenerateAnnualReportPdfAsync(int year)
        {
            try
            {
                // Get allocations
                var allocations = await _repository.GetFinancialAllocationsByYearAsync(year);

                // Get totals for last year
                var (totalDonations, totalAllocations, difference) = await GetLastYearTotalsAsync(year);

                //Get Detail
                // Get allocations Result
                var allocationResult = await GetFinancialAllocationsByYearAsync(year);

                if (!allocationResult.IsSuccess || allocationResult.Data == null || !allocationResult.Data.Any())
                {
                    throw new Exception($"No financial allocation data available for year {year}.");
                }

                var financialData = allocationResult.Data; // List<FinancialAllocationResponseDto>


                // Prepare pie data
                var pieData = allocations
                    .GroupBy(fa => fa.AllocationType.Name)
                    .Select(g => new
                    {
                        AllocationType = g.Key,
                        TotalAmount = g.Sum(x => x.Amount)
                    })
                    .ToList();

                // Grouped by year + allocation type
                var grouped = allocations
                    .GroupBy(fa => new { Year = fa.AllocationDate.Year, fa.AllocationType.Name })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        AllocationType = g.Key.Name,
                        TotalAmount = g.Sum(x => x.Amount)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.AllocationType)
                    .ToList();

                var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(40);

                        // Header
                        page.Header()
                            .Column(headerCol =>
                            {
                                headerCol.Item().Text("Disaster Guard")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2)
                                    .AlignCenter();
                                headerCol.Item().Text($"Financial Allocation Report For {year}")
                                    .FontSize(20)
                                    .SemiBold() 
                                    .AlignCenter();
                            });

                        // Content
                        page.Content().Column(column =>
                        {
                            column.Spacing(20);

                            // Pie chart
                            column.Item().AspectRatio(1).Svg(size =>
                            {
                                ScottPlot.Plot plot = new();

                                var values = pieData.Select(p => (double)p.TotalAmount).ToArray();
                                var labels = pieData.Select(p => p.AllocationType).ToArray();

                                var colors = new ScottPlot.Color[]
                                {
                            new ScottPlot.Color(Colors.Yellow.Medium.Hex),
                            new ScottPlot.Color(Colors.Green.Medium.Hex),
                            new ScottPlot.Color(Colors.Blue.Medium.Hex),
                            new ScottPlot.Color(Colors.Red.Medium.Hex),
                            new ScottPlot.Color(Colors.Orange.Medium.Hex)
                                };

                                var slices = new List<PieSlice>();
                                for (int i = 0; i < values.Length; i++)
                                {
                                    slices.Add(new PieSlice()
                                    {
                                        Value = values[i],
                                        FillColor = i < colors.Length ? colors[i] : ScottPlot.Colors.Gray,
                                        Label = labels[i]
                                    });
                                }

                                var pie = plot.Add.Pie(slices.ToArray());
                                pie.DonutFraction = 0.5;
                                pie.SliceLabelDistance = 1.5;
                                pie.LineColor = ScottPlot.Colors.White;
                                pie.LineWidth = 3;

                                foreach (var slice in pie.Slices)
                                {
                                    slice.LabelStyle.FontName = "Lato";
                                    slice.LabelStyle.FontSize = 16;
                                }

                                plot.Axes.Frameless();
                                plot.HideGrid();

                                return plot.GetSvgXml((int)size.Width, (int)size.Height);
                            });

                            // Summary table (your GetLastYearTotalsAsync result)
                            column.Item()
                                .PaddingTop(20)
                                .Text($"Summary for Last Year ({DateTime.Now.Year - 1})")
                                .FontSize(16)
                                .Underline()
                                .Bold();
                                

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(1).Padding(5).Text("Metric").SemiBold();
                                    header.Cell().Border(1).Padding(5).Text("Amount").SemiBold();
                                });

                                table.Cell().Border(1).Padding(5).Text("Total Donations");
                                table.Cell().Border(1).Padding(5).Text((totalDonations ?? 0).ToString("C"));

                                table.Cell().Border(1).Padding(5).Text("Total Allocations");
                                table.Cell().Border(1).Padding(5).Text((totalAllocations ?? 0).ToString("C"));

                                table.Cell().Border(1).Padding(5).Text("Difference");
                                table.Cell().Border(1).Padding(5).Text((difference ?? 0).ToString("C"));
                            });

                            // Detailed allocation table
                            column.Item()
                                .PaddingTop(20)
                                .Text("Detailed Allocation Table")
                                .FontSize(16)
                                .SemiBold()
                                .Underline();



                            column.Item().Table(table =>
                            {
                                // Define columns widths - adjust relative sizes as needed
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);  // Allocation Date (Year or full date)
                                    columns.RelativeColumn(2);  // Allocation Type Name
                                    //columns.RelativeColumn(1);  // Created By
                                    columns.RelativeColumn(3);  // Detail Name & Description (combine here)
                                    columns.RelativeColumn(2);  // Notes
                                    columns.RelativeColumn(1);  // Amount
                                });

                                // Header row with bold and border
                                table.Header(header =>
                                {
                                    header.Cell().Border(1).Padding(5).Text("Date").SemiBold();
                                    header.Cell().Border(1).Padding(5).Text("Allocation Type").SemiBold();
                                    //header.Cell().Border(1).Padding(5).Text("Created By").SemiBold();
                                    header.Cell().Border(1).Padding(5).Text("Details").SemiBold();
                                    header.Cell().Border(1).Padding(5).Text("Notes").SemiBold();
                                    header.Cell().Border(1).Padding(5).AlignRight().Text("Amount").SemiBold();
                                });

                                foreach (var item in financialData)
                                {
                                    // Format date as needed, e.g. "yyyy-MM-dd"
                                    string formattedDate = item.AllocationDate.ToString("yyyy-MM-dd");

                                    // Combine DetailName and DetailDescription (adjust formatting as preferred)
                                    string details = $"{item.DetailName}";
                                    if (!string.IsNullOrWhiteSpace(item.DetailDescription))
                                    {
                                        details += $"\n{item.DetailDescription}";
                                    }

                                    table.Cell().Border(1).Padding(5).Text(formattedDate);
                                    table.Cell().Border(1).Padding(5).Text(item.AllocationTypeName);
                                    //table.Cell().Border(1).Padding(5).Text(item.CreatedBy);
                                    table.Cell().Border(1).Padding(5).Text(details);
                                    table.Cell().Border(1).Padding(5).Text(item.Notes ?? "");
                                    table.Cell().Border(1).Padding(5).AlignRight().Text(item.Amount.ToString("C"));
                                }
                            });
                        });

                        // Footer
                        page.Footer().Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Span("Generated on ");
                                text.Span(DateTime.Now.ToString("dd MMMM yyyy"));
                            });

                            row.ConstantItem(50).AlignRight().Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                            });
                        });
                    });
                }).GeneratePdf();

                return pdfBytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate PDF report.", ex);
            }
        }


        private FinancialAllocationResponseDto MapToResponseDto(FinancialAllocation entity, string allocationTypeName)
        {
            return new FinancialAllocationResponseDto
            {
                AllocationId = entity.Id,
                //DonationId = entity.DonationId,
                AllocationTypeName = allocationTypeName,
                Amount = entity.Amount,
                AllocationDate = entity.AllocationDate,
                CreatedBy = entity.CreatedBy,
                Notes = entity.Notes,
                DetailName = entity.DetailName,
                DetailDescription = entity.DetailDescription
            };
        }


        Task<(decimal? TotalDonations, decimal? TotalAllocations, int TotalAllocationsCount, decimal? Difference)> IFinancialAllocationService.GetOverviewAsync(int year)
        {
            return _repository.GetOverviewAsync(year);
        }
    }
}
