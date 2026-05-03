using Application.DTOs;
using MediatR;

namespace Application.Commands.CriarRelatorio;

public record CriarRelatorioCommand(
         Guid AnaliseId,
         Guid DiagramaId,
         string? Nome,
         string? URLS3Relatorio

) : IRequest<RelatorioDto>;
