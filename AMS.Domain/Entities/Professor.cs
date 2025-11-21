using AMS.Domain.Common;
using AMS.Domain.Enums;
using AMS.Domain.ValueObjects;

namespace AMS.Domain.Entities;

public class Professor : Entity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UniversityIndex { get; set; } = string.Empty;
    public AcademicTitle AcademicTitle { get; set; } = AcademicTitle.None;

    public Address Address { get; set; } = null!;

    public Office? Office { get; set; }
}