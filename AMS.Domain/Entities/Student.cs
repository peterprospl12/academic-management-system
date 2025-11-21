using AMS.Domain.Common;
using AMS.Domain.ValueObjects;

namespace AMS.Domain.Entities;

public class Student : Entity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UniversityIndex { get; set; } = string.Empty;
    public int YearOfStudy { get; set; }

    public Address Address { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();
}