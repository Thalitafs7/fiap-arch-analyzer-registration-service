using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterAnaliseAllServico;

public record ObterAnaliseAllServicoQuery : IRequest<IEnumerable<AnaliseDto>>;
