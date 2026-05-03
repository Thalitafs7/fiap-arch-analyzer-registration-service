using Application.DTOs;
using MediatR;

namespace Application.Commands.DeletarAnalise;

public record DeletarAnaliseCommand(    
    Guid Id
    

) : IRequest<AnaliseDto>;
