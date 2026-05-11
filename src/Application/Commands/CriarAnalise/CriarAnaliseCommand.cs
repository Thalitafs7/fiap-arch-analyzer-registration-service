using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Commands.CriarAnalise;

public record CriarAnaliseCommand(
    Guid ClienteId,
    string? Descricao,
    string Nome,
    string Tipo,
    List<FileData> Files,
    string FileType

) : IRequest<AnaliseDto>;
