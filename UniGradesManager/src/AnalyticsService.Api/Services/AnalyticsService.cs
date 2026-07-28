using AnalyticsService.Clients;
using AnalyticsService.Dtos;
using AnalyticsService.Exceptions;

namespace AnalyticsService.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IGradesServiceClient _gradesClient;
        private readonly IUsersServiceClient _usersClient;
        private readonly ICoursesServiceClient _coursesClient;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            IGradesServiceClient gradesClient,
            IUsersServiceClient usersClient,
            ICoursesServiceClient coursesClient,
            ILogger<AnalyticsService> logger)
        {
            _gradesClient = gradesClient;
            _usersClient = usersClient;
            _coursesClient = coursesClient;
            _logger = logger;
        }

        public async Task<CourseStatsDto> GetCourseStatsAsync(int courseId, CancellationToken ct = default)
        {
            var grades = await _gradesClient.GetByCourseAsync(courseId, ct);
            var values = grades.Select(g => g.Value).ToList();
            var summary = GradeStatisticsCalculator.Calculate(values);

            return new CourseStatsDto
            {
                CourseId = courseId,
                Average = summary.Average,
                Median = summary.Median,
                Mode = summary.Mode,
                StdDev = summary.StdDev,
                Min = summary.Min,
                Max = summary.Max,
                PassRatePercent = summary.PassRatePercent,
                StudentCount = grades.Count,
            };
        }

        public async Task<List<DistributionItemDto>> GetCourseDistributionAsync(
            int courseId, CancellationToken ct = default)
        {
            var grades = await _gradesClient.GetByCourseAsync(courseId, ct);
            var values = grades.Select(g => g.Value).ToList();

            return GradeStatisticsCalculator.CalculateDistribution(values)
                .Select(d => new DistributionItemDto { Grade = d.Grade, Count = d.Count })
                .ToList();
        }

        public async Task<List<CourseGradeDto>> GetCourseGradesAsync(int courseId, CancellationToken ct = default)
        {
            var grades = await _gradesClient.GetByCourseAsync(courseId, ct);

            // One batched name lookup instead of one call per student.
            var namesById = await _usersClient.GetNamesByIdsAsync(
                grades.Select(g => g.StudentId), ct);

            return grades
                .Select(g => new CourseGradeDto
                {
                    StudentId = g.StudentId,
                    StudentName = namesById.GetValueOrDefault(g.StudentId, $"Студент #{g.StudentId}"),
                    Value = g.Value,
                })
                .OrderBy(g => g.StudentName)
                .ToList();
        }

        public async Task<StudentStatsDto> GetMyStatsAsync(int requestingUserId, CancellationToken ct = default)
        {
            var rollNumber = await ResolveOwnRollNumberAsync(requestingUserId, ct);
            return await GetStudentStatsByRollNumberAsync(rollNumber, ct);
        }

        public async Task<List<StudentGradeDto>> GetMyGradesAsync(int requestingUserId, CancellationToken ct = default)
        {
            var rollNumber = await ResolveOwnRollNumberAsync(requestingUserId, ct);
            return await GetStudentGradesByRollNumberAsync(rollNumber, ct);
        }

        public async Task<StudentStatsDto> GetStudentStatsByRollNumberAsync(
            int studentRollNumber, CancellationToken ct = default)
        {
            var grades = await _gradesClient.GetByStudentAsync(studentRollNumber, ct);
            var values = grades.Select(g => g.Value).ToList();

            return new StudentStatsDto
            {
                StudentId = studentRollNumber,
                Average = values.Count > 0 ? Math.Round(values.Average(), 2) : 0,
                GradedCourseCount = grades.Count,
            };
        }

        public async Task<List<StudentGradeDto>> GetStudentGradesByRollNumberAsync(
            int studentRollNumber, CancellationToken ct = default)
        {
            var grades = await _gradesClient.GetByStudentAsync(studentRollNumber, ct);

            var result = new List<StudentGradeDto>(grades.Count);
            foreach (var grade in grades)
            {
                // Sequential, not batched — Courses Service has no batch-by-ids
                // endpoint (unlike User Service). Fine at "courses one student
                // is graded in" scale; revisit if that list ever gets large.
                var course = await _coursesClient.GetCourseAsync(grade.CourseId, ct);
                if (course is null)
                {
                    _logger.LogWarning(
                        "Grade references CourseId {CourseId} which Courses Service doesn't have",
                        grade.CourseId);
                    continue;
                }

                result.Add(new StudentGradeDto
                {
                    CourseId = grade.CourseId,
                    CourseName = course.Name,
                    Period = course.Period,
                    Value = grade.Value,
                });
            }

            return result;
        }

        /// <summary>
        /// Translates a JWT identity (User.Id) into the roll number
        /// (User.StudentId) that Grades Service actually stores against —
        /// the two are deliberately different numbers in this system. See
        /// AnalyticsService.Api/README.md for why.
        /// </summary>
        private async Task<int> ResolveOwnRollNumberAsync(int requestingUserId, CancellationToken ct)
        {
            var rollNumber = await _usersClient.GetStudentRollNumberAsync(requestingUserId, ct);
            return rollNumber ?? throw new NoStudentRollNumberException(requestingUserId);
        }

    }
}
