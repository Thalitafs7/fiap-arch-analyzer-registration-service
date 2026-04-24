using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterOrdensAtivas;

public record ObterOrdensAtivasQuery : IRequest<IEnumerable<AnaliseDto>>;
