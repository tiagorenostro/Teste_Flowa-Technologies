namespace OrderGenerator.API.Messaging.DTO;

public sealed record ErrorDto(string Details, IEnumerable<FieldDto> Fields);
public sealed record FieldDto(string Name, string Message);