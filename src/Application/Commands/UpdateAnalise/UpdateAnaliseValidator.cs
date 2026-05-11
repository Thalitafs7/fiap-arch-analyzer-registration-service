using FluentValidation;

namespace Application.Commands.UpdateAnalise;

public class UpdateAnaliseValidator : AbstractValidator<UpdateAnaliseCommand>
{
    public UpdateAnaliseValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("ClienteId é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome da analise é obrigatório");



        RuleFor(x => x.Descricao)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Descricao))
            .WithMessage("Descrição deve ter no máximo 1000 caracteres.");
    }

}


