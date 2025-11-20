using AMS.Domain.Common;

namespace AMS.Domain.Entities;

public class Course : Entity
{
    public string Name { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public int EctsPoints { get; set; } = 0;
    
    public ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();
    public ICollection<Course> Prerequisites { get; private set; } = new List<Course>();
    public ICollection<Course> IsPrerequisiteFor { get; private set; } = new List<Course>();
}