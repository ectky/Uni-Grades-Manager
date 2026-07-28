using AnalyticsService.Dtos;
using AnalyticsService.Exceptions;
using AnalyticsService.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Controllers
{
    /// <summary>
    /// Приложение №1, Analytics Service section — the only service that
    /// exposes anything about grades to the frontend. Grades Service itself
    /// has no public endpoints at all (GradeService.Api/README.md).
    ///
    ///   GET /api/analytics/course/{id}                    — All
    ///   GET /api/analytics/course/{id}/distribution        — All
    ///   GET /api/analytics/course/{id}/grades              — Instructor, Admin
    ///   GET /api/analytics/student/me                       — Student (own data only — see below)
    ///   GET /api/analytics/student/me/grades                 — Student (own data only)
    ///   GET /api/analytics/student/{rollNumber}              — Instructor (roll-number lookup)
    ///   GET /api/analytics/student/{rollNumber}/grades        — Instructor (roll-number lookup)
    ///
    /// The /me split exists because User.Id (JWT identity) and
    /// User.StudentId (roll number, what Grades Service actually keys on)
    /// are two different numbers in this system — a Student logging in has
    /// no way to know or pass their own roll number, so "my data" resolves
    /// it server-side instead of taking it as a route parameter. See
    /// AnalyticsService.Api/README.md.
    /// </summary>
    [ApiController]
    [Route("api/analytics")]
    [Authorize] // every action here requires *some* authenticated role; specifics below
    public class AnalyticsController : ControllerBase
    {
        private readonly Services.IAnalyticsService _analyticsService;

        public AnalyticsController(Services.IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("course/{courseId:int}")]
        [ProducesResponseType(typeof(CourseStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CourseStatsDto>> GetCourseStats(int courseId, CancellationToken ct)
        {
            var stats = await _analyticsService.GetCourseStatsAsync(courseId, ct);
            return Ok(stats);
        }

        [HttpGet("course/{courseId:int}/distribution")]
        [ProducesResponseType(typeof(List<DistributionItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DistributionItemDto>>> GetCourseDistribution(
            int courseId, CancellationToken ct)
        {
            var distribution = await _analyticsService.GetCourseDistributionAsync(courseId, ct);
            return Ok(distribution);
        }

        [HttpGet("course/{courseId:int}/grades")]
        [Authorize(Roles = "Instructor,Admin")]
        [ProducesResponseType(typeof(List<CourseGradeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CourseGradeDto>>> GetCourseGrades(int courseId, CancellationToken ct)
        {
            var grades = await _analyticsService.GetCourseGradesAsync(courseId, ct);
            return Ok(grades);
        }

        /// <summary>The logged-in Student's own aggregate — no id in the URL, resolved from the JWT.</summary>
        [HttpGet("student/me")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(StudentStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentStatsDto>> GetMyStats(CancellationToken ct)
        {
            try
            {
                var userId = User.GetUserId();
                var stats = await _analyticsService.GetMyStatsAsync(userId, ct);
                return Ok(stats);
            }
            catch (NoStudentRollNumberException)
            {
                return NotFound();
            }
        }

        /// <summary>The logged-in Student's own per-course grades — no id in the URL, resolved from the JWT.</summary>
        [HttpGet("student/me/grades")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(List<StudentGradeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<StudentGradeDto>>> GetMyGrades(CancellationToken ct)
        {
            try
            {
                var userId = User.GetUserId();
                var grades = await _analyticsService.GetMyGradesAsync(userId, ct);
                return Ok(grades);
            }
            catch (NoStudentRollNumberException)
            {
                return NotFound();
            }
        }

        /// <summary>Instructor looking up a specific student by roll number — not a User.Id.</summary>
        [HttpGet("student/{rollNumber:int}")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(StudentStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<StudentStatsDto>> GetStudentStats(int rollNumber, CancellationToken ct)
        {
            var stats = await _analyticsService.GetStudentStatsByRollNumberAsync(rollNumber, ct);
            return Ok(stats);
        }

        /// <summary>Instructor looking up a specific student's grades by roll number — not a User.Id.</summary>
        [HttpGet("student/{rollNumber:int}/grades")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(List<StudentGradeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<StudentGradeDto>>> GetStudentGrades(int rollNumber, CancellationToken ct)
        {
            var grades = await _analyticsService.GetStudentGradesByRollNumberAsync(rollNumber, ct);
            return Ok(grades);
        }
    }
}
