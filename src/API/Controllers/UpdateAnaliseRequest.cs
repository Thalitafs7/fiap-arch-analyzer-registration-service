namespace API.Controllers;

public record UpdateAnaliseRequest(
    Guid AnaliseId,
    Guid ClienteId,
    string? Nome,
    string? Descricao
);
