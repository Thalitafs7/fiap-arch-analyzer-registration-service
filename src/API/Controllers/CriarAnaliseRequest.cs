namespace API.Controllers;

public record CriarAnaliseRequest(
    string? Nome,
    string? Tipo,
    string? Descricao,
    IFormFile File    
);
