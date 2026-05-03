using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterRelatorioAllServico;

public record ObterRelatorioAllServicoQuery : IRequest<IEnumerable<RelatorioDto>>;
