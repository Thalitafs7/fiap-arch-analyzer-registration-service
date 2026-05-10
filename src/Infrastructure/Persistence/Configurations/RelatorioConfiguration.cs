using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Infrastructure.Persistence.Configurations;

[ExcludeFromCodeCoverage]
public class RelatorioConfiguration : IEntityTypeConfiguration<Relatorio>
{
    public void Configure(EntityTypeBuilder<Relatorio> builder)
    {
        builder.ToTable("relatorio");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Nome).HasColumnName("nome");
        builder.Property(e => e.Soat_Analysis_Id).HasColumnName("soat_analysis_id");
        builder.Property(e => e.IdDiagrama).HasColumnName("id_diagrama");
        builder.Property(e => e.Message_Error).HasColumnName("message_error");

        builder.Property(e => e.Ativo).HasColumnName("ativo");
        builder.Property(e => e.DataCadastro).HasColumnName("data_cadastro");
        builder.Property(e => e.DataAtualizacao).HasColumnName("data_atualizacao");

        // Conversor para armazenar listas de string em JSONB (Postgres)
        var listStringConverter = new ValueConverter<List<string>?, string?>(
            v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrEmpty(v) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        builder.Property(e => e.Componentes_Identificado)
            .HasColumnName("componentes_identificado")
            .HasColumnType("jsonb")
            .HasConversion(listStringConverter);

        builder.Property(e => e.Risco_Arquitetura)
            .HasColumnName("risco_arquitetura")
            .HasColumnType("jsonb")
            .HasConversion(listStringConverter);

        builder.Property(e => e.Recomendacao)
            .HasColumnName("recomendacao")
            .HasColumnType("jsonb")
            .HasConversion(listStringConverter);

        // Relação com Diagrama (one-to-one). Se já mapeado no outro lado, repetir a configuração idêntica é aceitável.
        builder.HasOne(r => r.Diagrama)
               .WithOne(d => d.Relatorio)
               .HasForeignKey<Relatorio>(r => r.IdDiagrama);

    }
}
