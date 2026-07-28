using System.Diagnostics;
using System.Globalization;
using ClosedXML.Excel;
using ParserService.Clients;
using ParserService.Dtos;
using ParserService.Exceptions;

namespace ParserService.Services
{
    /// <summary>
    /// Validates and parses the Excel files described in Chapter 3.7 / 4.4:
    /// max 10 MB, .xlsx only, requires "StudentId" and "Value" columns.
    /// Writes the parsed grades to Grades Service (internal, Parser-only endpoint)
    /// after confirming the course exists via Courses Service.
    /// </summary>
    public class ExcelGradeParserService : IExcelGradeParserService
    {
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB — Chapter 4.4
        private const double MinGradeValue = 2.00;              // Bulgarian grading scale
        private const double MaxGradeValue = 6.00;
        private static readonly string[] AllowedExtensions = { ".xlsx" };
        private const string StudentIdColumnHeader = "StudentId";
        private const string ValueColumnHeader = "Value";

        private readonly ICoursesServiceClient _coursesClient;
        private readonly IGradesServiceClient _gradesClient;
        private readonly ILogger<ExcelGradeParserService> _logger;

        public ExcelGradeParserService(
            ICoursesServiceClient coursesClient,
            IGradesServiceClient gradesClient,
            ILogger<ExcelGradeParserService> logger)
        {
            _coursesClient = coursesClient;
            _gradesClient = gradesClient;
            _logger = logger;
        }

        public async Task<ParseResultDto> ParseAndUploadGradesAsync(
            int courseId, IFormFile file, CancellationToken ct = default)
        {
            var stopwatch = Stopwatch.StartNew();

            ValidateFile(file);

            // Parser Service only ever checks that the course exists —
            // it never creates or edits courses (Admin owns that, per Приложение №1).
            var course = await _coursesClient.GetCourseAsync(courseId, ct)
                ?? throw new CourseNotFoundException(courseId);

            var records = ParseWorkbook(file);

            await _gradesClient.UploadGradesAsync(new BatchGradeUploadDto
            {
                CourseId = courseId,
                Grades = records,
            }, ct);

            stopwatch.Stop();

            _logger.LogInformation(
                "Parsed and uploaded {Count} grade(s) for course {CourseId} in {ElapsedMs} ms",
                records.Count, courseId, stopwatch.ElapsedMilliseconds);

            return new ParseResultDto
            {
                CourseId = course.Id,
                CourseName = course.Name,
                Period = course.Period,
                RecordCount = records.Count,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Records = records,
            };
        }

        private static void ValidateFile(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new InvalidExcelStructureException("No file was uploaded.");

            if (file.Length > MaxFileSizeBytes)
                throw new FileTooLargeException(file.Length, MaxFileSizeBytes);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidExcelStructureException(
                    $"Unsupported file extension '{extension}'. Only .xlsx is supported.");
            }
        }

        private static List<GradeRecordDto> ParseWorkbook(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var workbook = OpenWorkbookOrThrow(stream);

            var worksheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidExcelStructureException("The workbook contains no worksheets.");

            var headerRow = worksheet.Row(1);
            var studentIdColumn = FindColumn(headerRow, StudentIdColumnHeader);
            var valueColumn = FindColumn(headerRow, ValueColumnHeader);

            if (studentIdColumn is null || valueColumn is null)
            {
                throw new InvalidExcelStructureException(
                    $"The file must contain a header row with '{StudentIdColumnHeader}' and '{ValueColumnHeader}' columns.");
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            var records = new List<GradeRecordDto>();
            var errors = new List<string>();

            for (int rowNum = 2; rowNum <= lastRow; rowNum++)
            {
                var row = worksheet.Row(rowNum);
                if (row.IsEmpty())
                    continue; // skip fully blank rows between data

                var studentIdCell = row.Cell(studentIdColumn.Value);
                var valueCell = row.Cell(valueColumn.Value);

                if (!TryReadStudentId(studentIdCell, out var studentId))
                {
                    errors.Add($"Row {rowNum}: invalid or missing {StudentIdColumnHeader}.");
                    continue;
                }

                if (!TryReadGradeValue(valueCell, out var value))
                {
                    errors.Add(
                        $"Row {rowNum}: invalid or missing {ValueColumnHeader} " +
                        $"(expected a number between {MinGradeValue:0.00} and {MaxGradeValue:0.00}).");
                    continue;
                }

                records.Add(new GradeRecordDto { StudentId = studentId, Value = value });
            }

            if (errors.Count > 0)
            {
                // Reject the whole file rather than partially importing it —
                // a half-uploaded gradebook is worse than none.
                throw new InvalidExcelStructureException(
                    "The file contains invalid rows: " + string.Join(" | ", errors));
            }

            if (records.Count == 0)
            {
                throw new InvalidExcelStructureException("The file does not contain any data rows.");
            }

            return records;
        }

        private static IXLWorkbook OpenWorkbookOrThrow(Stream stream)
        {
            try
            {
                return new XLWorkbook(stream);
            }
            catch (Exception)
            {
                throw new InvalidExcelStructureException(
                    "The file could not be read as a valid .xlsx workbook.");
            }
        }

        private static int? FindColumn(IXLRow headerRow, string headerName)
        {
            var lastCol = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
            for (int col = 1; col <= lastCol; col++)
            {
                var text = headerRow.Cell(col).GetString().Trim();
                if (string.Equals(text, headerName, StringComparison.OrdinalIgnoreCase))
                    return col;
            }
            return null;
        }

        private static bool TryReadStudentId(IXLCell cell, out int studentId)
        {
            studentId = 0;
            if (cell.IsEmpty()) return false;

            if (cell.DataType == XLDataType.Number)
            {
                var d = cell.GetDouble();
                if (d <= 0 || d != Math.Floor(d)) return false;
                studentId = (int)d;
                return true;
            }

            return int.TryParse(cell.GetString().Trim(), out studentId) && studentId > 0;
        }

        private static bool TryReadGradeValue(IXLCell cell, out double value)
        {
            value = 0;
            if (cell.IsEmpty()) return false;

            double parsed;
            if (cell.DataType == XLDataType.Number)
            {
                parsed = cell.GetDouble();
            }
            else if (!double.TryParse(
                cell.GetString().Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                return false;
            }

            if (parsed < MinGradeValue || parsed > MaxGradeValue) return false;

            value = Math.Round(parsed, 2);
            return true;
        }

        public byte[] GenerateTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Grades");

            worksheet.Cell(1, 1).Value = StudentIdColumnHeader;
            worksheet.Cell(1, 2).Value = ValueColumnHeader;
            worksheet.Row(1).Style.Font.Bold = true;

            // Example row to guide the instructor filling in the template.
            worksheet.Cell(2, 1).Value = 20231001;
            worksheet.Cell(2, 2).Value = 5.50;

            worksheet.Columns(1, 2).AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
