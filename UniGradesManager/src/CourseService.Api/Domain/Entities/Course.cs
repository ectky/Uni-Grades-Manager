namespace CourseService.Domain.Entities
{
    // NOTE: the version of this entity you originally shared also had
    // InitialGradeSubmission and FinalGradeSubmission (DateOnly) fields.
    // Those were removed later in the same conversation ("премахнати са") once
    // we established that Parser Service only checks course *existence* and
    // never touches deadlines. If you reintroduce per-course deadlines,
    // add the two DateOnly properties back here and in CourseDto/UpdateCourseDto.
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Period { get; set; } = default!;
        public int InstructorId { get; set; }
    }
}
