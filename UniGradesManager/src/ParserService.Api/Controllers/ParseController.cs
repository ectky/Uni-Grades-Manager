using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParserService.Dtos;
using ParserService.Exceptions;
using ParserService.Services;

namespace ParserService.Controllers
{
    /// <summary>
    /// Corresponds to Приложение №1, Parser Service section:
    ///   POST /api/parse            — Instructor
    ///   GET  /api/parse/template   — Instructor
    /// Parser Service only writes grades (via Grades Service) and only reads
    /// courses to confirm they exist — it never creates/edits either.
    /// </summary>
    [ApiController]
    [Route("api/parse")]
    [Authorize(Roles = "Instructor")]
    public class ParseController : ControllerBase
    {
        private readonly IExcelGradeParserService _parserService;
        private readonly ILogger<ParseController> _logger;

        public ParseController(IExcelGradeParserService parserService, ILogger<ParseController> logger)
        {
            _parserService = parserService;
            _logger = logger;
        }

        /// <summary>
        /// Accepts a single .xlsx file (multipart/form-data) plus the target CourseId,
        /// parses it, verifies the course exists, and writes the grades to Grades Service.
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB — matches the 4.4 limit; also enforced in the service
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ParseResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        public async Task<ActionResult<ParseResultDto>> ParseAndUploadGrades(
            [FromForm] ParseGradesRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _parserService.ParseAndUploadGradesAsync(request.CourseId, request.File, ct);
                return Ok(result);
            }
            catch (InvalidExcelStructureException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Excel structure",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (CourseNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Course not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                });
            }
            catch (FileTooLargeException ex)
            {
                return StatusCode(StatusCodes.Status413PayloadTooLarge, new ProblemDetails
                {
                    Title = "File too large",
                    Detail = ex.Message,
                    Status = StatusCodes.Status413PayloadTooLarge,
                });
            }
            catch (GradesUploadFailedException ex)
            {
                _logger.LogError(ex, "Failed to upload parsed grades for course {CourseId}", request.CourseId);
                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Title = "Grades Service unavailable",
                    Detail = "The parsed file was valid, but Grades Service rejected or failed to store the data. Please try again.",
                    Status = StatusCodes.Status502BadGateway,
                });
            }
        }

        /// <summary>Downloads a blank .xlsx template with the required column headers.</summary>
        [HttpGet("template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DownloadTemplate()
        {
            var bytes = _parserService.GenerateTemplate();
            const string contentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, "grades_template.xlsx");
        }
    }
}
