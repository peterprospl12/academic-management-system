using AMS.Domain.Common;

namespace AMS.Domain.Entities;

public class Office : Entity
{
    public string RoomNumber { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;

    public Guid? ProfessorId { get; set; }
    public Professor? Professor { get; set; }
}