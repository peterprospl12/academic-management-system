namespace AMS.Application.DTOs;

public record CreateSequenceDto(string Prefix, int InitialValue);

public record SequenceDto(string Prefix, int InitialValue);

public record UpdateSequenceDto(
    string Prefix,
    int NewValue
);