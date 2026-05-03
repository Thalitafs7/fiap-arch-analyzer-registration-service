using FluentValidation;

namespace Application.Commands.CriarRelatorio;

public class CriarRelatorioValidator : AbstractValidator<CriarRelatorioCommand>
{
    public CriarRelatorioValidator()
    {
        RuleFor(x => x.DiagramaId)
            .NotEmpty()
            .WithMessage("DiagramaId é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome da analise é obrigatório");

        RuleFor(x => x.URLS3Relatorio)
            .NotEmpty()
            .WithMessage("URLS3 do relátorio é obrigátorio");

    }
}


