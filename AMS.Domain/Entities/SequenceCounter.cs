namespace AMS.Domain.Entities;

public class SequenceCounter
{
    public string Prefix { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
}