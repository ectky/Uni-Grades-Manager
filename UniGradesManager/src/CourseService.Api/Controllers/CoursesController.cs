using CourseService.Dtos;
using CourseService.Exceptions;
using CourseService.Extensions;
using CourseService.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.Controllers
{
    /// <summary>
    /// Приложение №1, Courses Service section:
    ///   GET  /api/courses           — Admin
    ///   GET  /api/courses/my        — Instructor  (see note below — Student was dropped)
    ///   POST /api/courses           — Admin
    ///   PUT  /api/courses/{id}      — Admin, Instructor-собственик
    ///   GET  /api/courses/{id}      — internal only (Parser Service, Analytics Service)
    ///
    /// NOTE on GET /api/courses/my and the "Student" role: an earlier version of
    /// this endpoint list also gave Students access here. That doesn't hold up —
    /// without an Enrollments table, Courses Service has no way to know which
    /// courses belong to a given student; only Grades Service knows that (via
    /// the presence of a Grade row), and Analytics Service already exposes it,
    /// enriched, as GET /api/analytics/student/{id}/grades. So a Student's
    /// "my courses" view is served by Analytics Service, not here. If you want
    /// Students to keep hitting this endpoint directly, you're back to needing
    /// an Enrollments table — flag it back to me and I'll wire that up instead.
    /// </summary>
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly Services.ICourseService _courseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(Services.ICourseService courseService, ILogger<CoursesController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<CourseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CourseDto>>> GetAll(CancellationToken ct)
        {
            var courses = await _courseService.GetAllAsync(ct);
            return Ok(courses);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(List<CourseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CourseDto>>> GetMyCourses(CancellationToken ct)
        {
            var instructorId = User.GetUserId();
            var courses = await _courseService.GetByInstructorAsync(instructorId, ct);
            return Ok(courses);
        }

        /// <summary>Internal-only lookup — Parser Service's existence check, Analytics Service's enrichment.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [InternalOnly]
        [ProducesResponseType(typeof(CourseSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseSummaryDto>> GetSummary(int id, CancellationToken ct)
        {
            try
            {
                var summary = await _courseService.GetSummaryAsync(id, ct);
                return Ok(summary);
            }
            catch (CourseNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _courseService.CreateAsync(dto, ct);
                return CreatedAtAction(nameof(GetSummary), new { id = created.Id }, created);
            }
            catch (InvalidCourseException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid course",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseDto>> Update(int id, [FromBody] UpdateCourseDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var role = User.IsInRole("Admin") ? "Admin" : "Instructor";

            try
            {
                var updated = await _courseService.UpdateAsync(id, dto, userId, role, ct);
                return Ok(updated);
            }
            catch (CourseNotFoundException)
            {
                return NotFound();
            }
            catch (ForbiddenCourseAccessException ex)
            {
                _logger.LogWarning(ex, "Blocked course edit attempt by user {UserId}", userId);
                return Forbid();
            }
            catch (InvalidCourseException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid course",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                });
            }
        }
    }
}
