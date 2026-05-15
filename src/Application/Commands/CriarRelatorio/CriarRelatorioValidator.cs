using FluentValidation;

namespace Application.Commands.CriarRelatorio;

public class CriarRelatorioValidator : AbstractValidator<CriarRelatorioCommand>
{
    public CriarRelatorioValidator()
    {
        RuleFor(x => x.AnalysisId)
            .NotEmpty()
            .WithMessage("Id Analise é obrigatório.");

        RuleFor(x => x.Report!.ExecutiveSummary)
            .NotEmpty()
            .WithMessage("Nome da analise é obrigatório")
            .When(x => x.Report != null);
    }
}


