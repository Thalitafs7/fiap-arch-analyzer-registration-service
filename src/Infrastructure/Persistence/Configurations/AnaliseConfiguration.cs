using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Persistence.Configurations;

[ExcludeFromCodeCoverage]
public class AnaliseConfiguration : IEntityTypeConfiguration<Analise>
{
    public void Configure(EntityTypeBuilder<Analise> builder)
    {
        builder.ToTable("analise");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Nome).HasColumnName("nome");
        builder.Property(e => e.Status).HasColumnName("status");

        builder.Property(e => e.Ativo).HasColumnName("ativo");
        builder.Property(e => e.DataCadastro).HasColumnName("data_cadastro");
        builder.Property(e => e.DataAtualizacao).HasColumnName("data_atualizacao");

    }
}
