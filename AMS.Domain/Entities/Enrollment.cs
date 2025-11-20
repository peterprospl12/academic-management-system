using AMS.Domain.Common;

namespace AMS.Domain.Entities;

public class Enrollment : Entity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid? CourseId { get; set; }
    public Course? Course { get; set; } = null!;

    public double? Grade { get; set; }
    public string Semester { get; set; } = string.Empty;
}