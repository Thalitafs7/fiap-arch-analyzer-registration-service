using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.ExternalServices;

[ExcludeFromCodeCoverage]
public class SagaSettings
{
    public const string SectionName = "Saga";

    public int TimeoutMinutes { get; set; } = 15;
}
