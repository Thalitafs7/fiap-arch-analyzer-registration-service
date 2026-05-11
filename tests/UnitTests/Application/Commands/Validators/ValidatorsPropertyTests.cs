using Application.Commands.CriarAnalise;
using Application.Commands.CriarRelatorio;
using Application.Commands.DeletarAnalise;
using Application.Commands.UpdateAnalise;
using Application.Common.Models;
using FsCheck;
using FsCheck.Xunit;
using UnitTests.Generators;

namespace UnitTests.Application.Commands.Validators;

/// <summary>
/// Property-Based Tests for all FluentValidation validators.
///
/// **Validates: Requirements 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8, 4.9, 4.10, 4.11**
/// </summary>
public class ValidatorsPropertyTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // CriarAnaliseValidator
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Req 4.1 — ClienteId vazio → falha com mensagem contendo "ClienteId".
    /// </summary>
    [Property(MaxTest = 50)]
    public Property CriarAnaliseValidator_EmptyClienteId_FailsWithClienteIdMessage()
    {
        var arb = (from nome in DomainGenerators.ValidNome().Generator
                   from ext in DomainGenerators.AllowedExtension().Generator
                   select (nome, ext)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (nome, ext) = tuple;
            var validator = new CriarAnaliseValidator();
            var file = new FileData(new byte[100], $"file{ext}", "image/png");
            var command = new CriarAnaliseCommand(
                Guid.Empty, "desc", nome, "Arquitetura", new List<FileData> { file }, "png");

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("ClienteId"));
        });
    }

    /// <summary>
    /// Req 4.2 — Nome vazio → falha com mensagem contendo "Nome".
    /// </summary>
    [Property(MaxTest = 50)]
    public Property CriarAnaliseValidator_EmptyNome_FailsWithNomeMessage()
    {
        var arb = (from clienteId in DomainGenerators.ValidGuid().Generator
                   from ws in DomainGenerators.WhitespaceString().Generator
                   from ext in DomainGenerators.AllowedExtension().Generator
                   select (clienteId, ws, ext)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (clienteId, ws, ext) = tuple;
            var validator = new CriarAnaliseValidator();
            var file = new FileData(new byte[100], $"file{ext}", "image/png");
            var command = new CriarAnaliseCommand(
                clienteId, "desc", ws, "Arquitetura", new List<FileData> { file }, "png");

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("Nome"));
        });
    }

    /// <summary>
    /// Req 4.3 — Extensão inválida → falha na validação de arquivo.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property CriarAnaliseValidator_DisallowedExtension_FailsFileTypeValidation()
    {
        var arb = (from clienteId in DomainGenerators.ValidGuid().Generator
                   from nome in DomainGenerators.ValidNome().Generator
                   from ext in DomainGenerators.DisallowedExtension().Generator
                   select (clienteId, nome, ext)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (clienteId, nome, ext) = tuple;
            var validator = new CriarAnaliseValidator();
            var file = new FileData(new byte[100], $"file{ext}", "application/octet-stream");
            var command = new CriarAnaliseCommand(
                clienteId, "desc", nome, "Arquitetura", new List<FileData> { file }, "png");

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("Files"));
        });
    }

    /// <summary>
    /// Req 4.4 — Arquivo > 10MB → falha na validação de tamanho.
    /// </summary>
    [Property(MaxTest = 20)]
    public Property CriarAnaliseValidator_FileSizeExceeds10MB_FailsSizeValidation()
    {
        var arb = (from clienteId in DomainGenerators.ValidGuid().Generator
                   from nome in DomainGenerators.ValidNome().Generator
                   from ext in DomainGenerators.AllowedExtension().Generator
                   select (clienteId, nome, ext)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (clienteId, nome, ext) = tuple;
            var validator = new CriarAnaliseValidator();
            // 10MB + 1 byte
            var oversizedContent = new byte[10 * 1024 * 1024 + 1];
            var file = new FileData(oversizedContent, $"file{ext}", "image/png");
            var command = new CriarAnaliseCommand(
                clienteId, "desc", nome, "Arquitetura", new List<FileData> { file }, "png");

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("Files"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CriarRelatorioValidator
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Req 4.5 — AnalysisId vazio → falha.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property CriarRelatorioValidator_EmptyAnalysisId_Fails()
    {
        var arb = (from summary in Arb.Generate<NonEmptyString>().Select(s => s.Get)
                   select summary).ToArbitrary();

        return Prop.ForAll(arb, summary =>
        {
            var validator = new CriarRelatorioValidator();
            var report = new ReportDetail { ExecutiveSummary = summary };
            var command = new CriarRelatorioCommand(
                Guid.Empty, Guid.NewGuid(), "completed", report, null, null);

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("AnalysisId"));
        });
    }

    /// <summary>
    /// Req 4.6 — ExecutiveSummary vazio → falha.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property CriarRelatorioValidator_EmptyExecutiveSummary_Fails()
    {
        var arb = (from analysisId in DomainGenerators.ValidGuid().Generator
                   from ws in DomainGenerators.WhitespaceString().Generator
                   select (analysisId, ws)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (analysisId, ws) = tuple;
            var validator = new CriarRelatorioValidator();
            var report = new ReportDetail { ExecutiveSummary = ws };
            var command = new CriarRelatorioCommand(
                analysisId, Guid.NewGuid(), "completed", report, null, null);

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("ExecutiveSummary"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DeletarAnaliseValidator
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Req 4.7 — Id vazio → falha.
    /// </summary>
    [Fact]
    public void DeletarAnaliseValidator_EmptyId_Fails()
    {
        var validator = new DeletarAnaliseValidator();
        var command = new DeletarAnaliseCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Id"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UpdateAnaliseValidator
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Req 4.8 — ClienteId vazio → falha.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property UpdateAnaliseValidator_EmptyClienteId_Fails()
    {
        return Prop.ForAll(DomainGenerators.ValidNome(), nome =>
        {
            var validator = new UpdateAnaliseValidator();
            var command = new UpdateAnaliseCommand(Guid.Empty, Guid.NewGuid(), "desc", nome);

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("ClienteId"));
        });
    }

    /// <summary>
    /// Req 4.9 — Nome vazio → falha.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property UpdateAnaliseValidator_EmptyNome_Fails()
    {
        var arb = (from clienteId in DomainGenerators.ValidGuid().Generator
                   from ws in DomainGenerators.WhitespaceString().Generator
                   select (clienteId, ws)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (clienteId, ws) = tuple;
            var validator = new UpdateAnaliseValidator();
            var command = new UpdateAnaliseCommand(clienteId, Guid.NewGuid(), "desc", ws);

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("Nome"));
        });
    }

    /// <summary>
    /// Req 4.10 — Descricao > 1000 chars → falha.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property UpdateAnaliseValidator_LongDescricao_Fails()
    {
        var arb = (from clienteId in DomainGenerators.ValidGuid().Generator
                   from nome in DomainGenerators.ValidNome().Generator
                   from longDesc in DomainGenerators.LongString().Generator
                   select (clienteId, nome, longDesc)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (clienteId, nome, longDesc) = tuple;
            var validator = new UpdateAnaliseValidator();
            var command = new UpdateAnaliseCommand(clienteId, Guid.NewGuid(), longDesc, nome);

            var result = validator.Validate(command);

            return !result.IsValid
                && result.Errors.Any(e => e.PropertyName.Contains("Descricao"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Req 4.11 — Comandos válidos retornam IsValid=true
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Req 4.11 — CriarAnaliseValidator com comando válido → IsValid=true.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property CriarAnaliseValidator_ValidCommand_IsValid()
    {
        var arb = (from clienteId in DomainGenerators.ValidGuid().Generator
                   from nome in DomainGenerators.ValidNome().Generator
                   from ext in DomainGenerators.AllowedExtension().Generator
                   select (clienteId, nome, ext)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (clienteId, nome, ext) = tuple;
            var validator = new CriarAnaliseValidator();
            var file = new FileData(new byte[100], $"file{ext}", "image/png");
            var command = new CriarAnaliseCommand(
                clienteId, "desc", nome, "Arquitetura", new List<FileData> { file }, "png");

            var result = validator.Validate(command);

            return result.IsValid;
        });
    }

    /// <summary>
    /// Req 4.11 — CriarRelatorioValidator com comando válido → IsValid=true.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property CriarRelatorioValidator_ValidCommand_IsValid()
    {
        var arb = (from analysisId in DomainGenerators.ValidGuid().Generator
                   from summary in Arb.Generate<NonEmptyString>().Select(s => s.Get)
                                      .Where(s => !string.IsNullOrWhiteSpace(s))
                   select (analysisId, summary)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (analysisId, summary) = tuple;
            var validator = new CriarRelatorioValidator();
            var report = new ReportDetail { ExecutiveSummary = summary };
            var command = new CriarRelatorioCommand(
                analysisId, Guid.NewGuid(), "completed", report, null, null);

            var result = validator.Validate(command);

            return result.IsValid;
        });
    }

    /// <summary>
    /// Req 4.11 — DeletarAnaliseValidator com Id válido → IsValid=true.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property DeletarAnaliseValidator_ValidId_IsValid()
    {
        return Prop.ForAll(DomainGenerators.ValidGuid(), id =>
        {
            var validator = new DeletarAnaliseValidator();
            var command = new DeletarAnaliseCommand(id);

            var result = validator.Validate(command);

            return result.IsValid;
        });
    }

    /// <summary>
    /// Req 4.11 — UpdateAnaliseValidator com comando válido → IsValid=true.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property UpdateAnaliseValidator_ValidCommand_IsValid()
    {
        return Prop.ForAll(DomainGenerators.ValidUpdateAnaliseCommand(), command =>
        {
            var validator = new UpdateAnaliseValidator();
            var result = validator.Validate(command);
            return result.IsValid;
        });
    }
}
