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

        // Validação de tipo/ extensão do arquivo: aceita .pdf, .jpeg, .png
        RuleFor(x => x.FileType)
            .NotEmpty()
            .WithMessage("Tipo de arquivo é obrigátorio")
            .Must(BeAllowedFileType)
            .WithMessage("Somente arquivos com extensão .pdf, .jpeg ou .png são permitidos.");
    }


    private bool BeAllowedFileType(string? fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType))
            return false;

        // Primeiro, tenta extrair extensão de nome de arquivo (.pdf, .jpeg, .png)
        var ext = Path.GetExtension(fileType);
        if (!string.IsNullOrEmpty(ext))
        {
            var normalized = ext.TrimStart('.').ToLowerInvariant();
            return normalized == "pdf" || normalized == "jpeg" || normalized == "png";
        }

        // Fallback: suportar também MIME types comuns
        var lower = fileType.Trim().ToLowerInvariant();
        return lower == "application/pdf" || lower == "image/png" || lower == "image/jpeg";
    }

}


