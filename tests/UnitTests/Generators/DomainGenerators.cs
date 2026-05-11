using Application.Commands.AtualizarStatusAnalise;
using Application.Commands.UpdateAnalise;
using Domain.Entities;
using Domain.Enums;
using FsCheck;

namespace UnitTests.Generators;

public static class DomainGenerators
{
    // --- Primitives ---
    public static Arbitrary<Guid> ValidGuid() =>
        Arb.Generate<Guid>().Where(g => g != Guid.Empty).ToArbitrary();

    public static Arbitrary<string> ValidNome() =>
        Arb.Generate<NonEmptyString>()
            .Select(s => s.Get)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArbitrary();

    public static Arbitrary<string> WhitespaceString() =>
        Gen.Elements("", " ", "  ", "\t", "\n", "   \t\n")
            .ToArbitrary();

    public static Arbitrary<StatusAnalise> AnyStatus() =>
        Gen.Elements(
            StatusAnalise.Recebido,
            StatusAnalise.EmProcessamento,
            StatusAnalise.Analisado,
            StatusAnalise.Error)
            .ToArbitrary();

    // --- Domain Entities ---
    public static Arbitrary<Analise> ValidAnalise() =>
        (from clienteId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
         from nome in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
         from status in Gen.Elements(StatusAnalise.Recebido, StatusAnalise.EmProcessamento, StatusAnalise.Analisado, StatusAnalise.Error)
         from diagramaCount in Gen.Choose(0, 3)
         let diagramas = Enumerable.Range(0, diagramaCount)
             .Select(i => new Diagrama($"diag_{i}.png", "image/png", $"s3://bucket/diag_{i}.png"))
             .ToList()
         select new Analise(clienteId, nome, status, diagramas, "desc"))
        .ToArbitrary();

    public static Arbitrary<Diagrama> ValidDiagrama() =>
        (from nome in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
         from tipo in Gen.Elements("image/png", "image/jpeg", "application/pdf")
         from url in Arb.Generate<NonEmptyString>().Select(s => $"s3://bucket/{s.Get}")
         select new Diagrama(nome, tipo, url))
        .ToArbitrary();

    public static Arbitrary<Relatorio> ValidRelatorio() =>
        (from nome in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
         from soatId in Arb.Generate<Guid>()
         from diagramaId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
         from compCount in Gen.Choose(0, 3)
         from riskCount in Gen.Choose(0, 3)
         from recCount in Gen.Choose(0, 3)
         let comps = Enumerable.Range(0, compCount).Select(i => $"comp_{i}").ToList()
         let risks = Enumerable.Range(0, riskCount).Select(i => $"risk_{i}").ToList()
         let recs = Enumerable.Range(0, recCount).Select(i => $"rec_{i}").ToList()
         select new Relatorio(nome, soatId, diagramaId, comps, risks, recs))
        .ToArbitrary();

    // --- File count for commands ---
    public static Arbitrary<int> ValidFileCount() =>
        Gen.Choose(1, 5).ToArbitrary();

    // --- Webhook helpers ---
    public static Arbitrary<string> ValidSecret() =>
        Arb.Generate<NonEmptyString>()
            .Select(s => s.Get)
            .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length >= 8)
            .ToArbitrary();

    // --- Commands ---
    public static Arbitrary<UpdateAnaliseCommand> ValidUpdateAnaliseCommand() =>
        (from clienteId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
         from analiseId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
         from nome in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
         from descricao in Arb.Generate<NonEmptyString>().Select(s => s.Get)
         select new UpdateAnaliseCommand(clienteId, analiseId, descricao, nome))
        .ToArbitrary();

    public static Arbitrary<AtualizarStatusAnaliseCommand> ValidAtualizarStatusCommand() =>
        (from id in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
         select new AtualizarStatusAnaliseCommand(id))
        .ToArbitrary();

    // --- Exceptions ---
    public static Arbitrary<Exception> AnyException() =>
        Gen.Elements<Exception>(
            new InvalidOperationException("test"),
            new ArgumentException("test"),
            new Exception("generic"))
        .ToArbitrary();

    // --- Strings > 1000 chars ---
    public static Arbitrary<string> LongString() =>
        Gen.Choose(1001, 2000)
           .Select(len => new string('x', len))
           .ToArbitrary();

    // --- File extensions ---
    public static Arbitrary<string> DisallowedExtension() =>
        Gen.Elements(".exe", ".bat", ".sh", ".dll", ".zip", ".rar", ".js", ".html")
           .ToArbitrary();

    public static Arbitrary<string> AllowedExtension() =>
        Gen.Elements(".pdf", ".jpeg", ".png", ".jpg")
           .ToArbitrary();
}
