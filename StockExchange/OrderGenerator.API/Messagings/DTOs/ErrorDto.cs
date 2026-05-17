namespace OrderGenerator.API.Messagings.DTOs;

public sealed record ErrorDto(string Details, IEnumerable<FieldDto> Fields);
public sealed record FieldDto(string Name, string Message);