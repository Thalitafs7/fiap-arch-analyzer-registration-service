using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.CriarAnalise;

public record CriarAnaliseCommand(
    Guid ClienteId,
    string? Descricao,
    string Nome,
    string Tipo,
    List<IFormFile> Files,
    string FileType

) : IRequest<AnaliseDto>;
