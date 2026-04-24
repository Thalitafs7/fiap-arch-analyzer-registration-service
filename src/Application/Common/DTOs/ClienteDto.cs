namespace Application.Common.DTOs;

public record ClienteDto(
    Guid Id,
    string Nome,
    string Email
);
