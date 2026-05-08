namespace OrderGenerator.API.Infrastructure.DTOs;

public sealed record ErrorDto(string Details, IEnumerable<Field> Fields);
public sealed record Field(string Name, string Message);