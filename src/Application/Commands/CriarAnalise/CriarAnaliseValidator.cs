using FluentValidation;

namespace Application.Commands.CriarAnalise;

public class CriarAnaliseValidator : AbstractValidator<CriarAnaliseCommand>
{
    public CriarAnaliseValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("ClienteId é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome da analise é obrigatório");

        RuleFor(x => x.FileType)
            .NotEmpty()
            .WithMessage("Tipo de arquivo é obrigátorio");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Descricao))
            .WithMessage("Descrição deve ter no máximo 1000 caracteres.");

        RuleFor(x => x.FileType).NotNull()
            .WithMessage("Arquivo é obrigátorio");
    }
}


