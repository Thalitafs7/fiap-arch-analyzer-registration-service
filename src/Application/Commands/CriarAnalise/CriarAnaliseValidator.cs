using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.CriarAnalise;

public class CriarAnaliseValidator : AbstractValidator<CriarAnaliseCommand>
{

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
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

        // Valida cada arquivo da coleção: só permite .pdf, .jpeg, .png e .docx
        RuleForEach(x => x.Files)
            .Must(BeAllowedFileType)
            .WithMessage("Somente arquivos com extensão .pdf, .jpeg, .png ou .docx são permitidos.")
            .Must(file => file != null && file.Length <= MaxFileSizeBytes)
            .WithMessage($"Cada arquivo deve ter no máximo {MaxFileSizeBytes / (1024 * 1024)}MB.");
    }

    private bool BeAllowedFileType(IFormFile? file)
    {
        if (file == null) return false;

        // Tenta extrair extensão do nome do arquivo
        var ext = Path.GetExtension(file.FileName);
        if (!string.IsNullOrEmpty(ext))
        {
            var normalized = ext.TrimStart('.').ToLowerInvariant();
            return normalized == "pdf"
                || normalized == "jpeg"
                || normalized == "png"
                || normalized == "jpg";
        }

        // Fallback para content-type (MIME)
        var ct = file.ContentType?.Trim().ToLowerInvariant();
        return ct == "application/pdf"
            || ct == "image/png"
            || ct == "image/jpeg"
            || ct == "image/jpg";
    }

}


