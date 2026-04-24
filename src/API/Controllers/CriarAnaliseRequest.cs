namespace API.Controllers;

public record CriarAnaliseRequest(
    string? Nome,
    string? Tipo,
    string? Descricao,
    List<IFormFile> Files,
    FileTypeEnum FileType
);
