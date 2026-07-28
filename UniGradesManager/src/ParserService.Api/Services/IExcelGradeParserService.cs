using Microsoft.AspNetCore.Http;
using ParserService.Dtos;

namespace ParserService.Services
{
    public interface IExcelGradeParserService
    {
        /// <summary>
        /// Validates the workbook, confirms the course exists, writes the parsed
        /// grades to Grades Service, and returns a summary for the confirmation screen.
        /// </summary>
        Task<ParseResultDto> ParseAndUploadGradesAsync(int courseId, IFormFile file, CancellationToken ct = default);

        /// <summary>Generates a blank .xlsx template with the expected column headers.</summary>
        byte[] GenerateTemplate();
    }
}
