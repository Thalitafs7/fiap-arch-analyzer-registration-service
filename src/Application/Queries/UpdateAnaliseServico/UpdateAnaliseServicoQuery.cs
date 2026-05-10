using Application.DTOs;
using MediatR;

namespace Application.Queries.UpdateAnaliseServico;

public record UpdateAnaliseServicoQuery(Guid Id) : IRequest<AnaliseDto>;
