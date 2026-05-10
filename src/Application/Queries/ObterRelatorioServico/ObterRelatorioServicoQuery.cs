using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterRelatorioServico;

public record ObterRelatorioServicoQuery(Guid Id) : IRequest<RelatorioDto>;
