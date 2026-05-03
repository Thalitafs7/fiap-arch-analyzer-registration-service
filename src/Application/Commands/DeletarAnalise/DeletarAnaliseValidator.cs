using FluentValidation;

namespace Application.Commands.DeletarAnalise;

public class DeletarAnaliseValidator : AbstractValidator<DeletarAnaliseCommand>
{
    public DeletarAnaliseValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id é obrigatório.");

    }

}


