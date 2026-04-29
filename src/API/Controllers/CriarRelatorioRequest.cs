using Domain.Entities;

namespace API.Controllers;

public record CriarRelatorioRequest(
         Guid AnaliseId,
         Guid DiagramaId,
         string? Nome,
         string? URLS3Relatorio
);
