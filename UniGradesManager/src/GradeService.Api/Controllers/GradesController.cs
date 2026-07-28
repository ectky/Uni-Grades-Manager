using GradeService.Dtos;
using GradeService.Exceptions;
using GradeService.Security;
using Microsoft.AspNetCore.Mvc;

namespace GradeService.Controllers
{
    /// <summary>
    /// Grades Service has no public, user-facing endpoints. All three actions
    /// here are internal, service-to-service only (see InternalOnlyAttribute):
    ///   POST /api/grades            — called only by Parser Service
    ///   GET  /api/grades/course/{id}   — called only by Analytics Service
    ///   GET  /api/grades/student/{id}  — called only by Analytics Service
    /// Students, instructors, and admins never call this service directly —
    /// they read through Analytics Service, which aggregates and enriches
    /// this raw data with names/periods from User Service and Courses Service.
    /// </summary>
    [ApiController]
    [Route("api/grades")]
    [InternalOnly]
    public class GradesController : ControllerBase
    {
        private readonly Services.IGradeService _gradeService;
        private readonly ILogger<GradesController> _logger;

        public GradesController(Services.IGradeService gradeService, ILogger<GradesController> logger)
        {
            _gradeService = gradeService;
            _logger = logger;
        }

        /// <summary>
        /// Upserts a batch of grades for one course: existing (CourseId, StudentId)
        /// rows are updated in place, new ones are inserted. Called once per
        /// Excel upload by Parser Service — never partially, never by the frontend.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BatchGradeUploadResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BatchGradeUploadResultDto>> UploadGrades(
            [FromBody] BatchGradeUploadDto batch, CancellationToken ct)
        {
            try
            {
                var result = await _gradeService.UploadGradesAsync(batch, ct);
                return Ok(result);
            }
            catch (InvalidGradeBatchException ex)
            {
                _logger.LogWarning(ex, "Rejected invalid grade batch for course {CourseId}", batch.CourseId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid grade batch",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                });
            }
        }

        /// <summary>Raw grades for a course — consumed by Analytics Service only.</summary>
        [HttpGet("course/{courseId:int}")]
        [ProducesResponseType(typeof(List<GradeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GradeDto>>> GetByCourse(int courseId, CancellationToken ct)
        {
            var grades = await _gradeService.GetByCourseAsync(courseId, ct);
            return Ok(grades);
        }

        /// <summary>Raw grades for a student across all their courses — consumed by Analytics Service only.</summary>
        [HttpGet("student/{studentId:int}")]
        [ProducesResponseType(typeof(List<GradeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GradeDto>>> GetByStudent(int studentId, CancellationToken ct)
        {
            var grades = await _gradeService.GetByStudentAsync(studentId, ct);
            return Ok(grades);
        }
    }
}
