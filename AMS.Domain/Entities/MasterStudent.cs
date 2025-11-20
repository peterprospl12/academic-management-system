namespace AMS.Domain.Entities;

public class MasterStudent : Student
{
    public string ThesisTopic { get; set; } = string.Empty;
    
    public Guid? PromoterId { get; set; }
    public Professor? Promoter { get; set; } = null!;
}