using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Persistence.Configurations;

[ExcludeFromCodeCoverage]
public class DiagramaConfiguration : IEntityTypeConfiguration<Diagrama>
{
    public void Configure(EntityTypeBuilder<Diagrama> builder)
    {
        builder.ToTable("diagrama");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");


        builder.Property(e => e.Ativo).HasColumnName("ativo");
        builder.Property(e => e.DataCadastro).HasColumnName("data_cadastro");
        builder.Property(e => e.DataAtualizacao).HasColumnName("data_atualizacao");


        builder.HasOne(e => e.Relatorio)
            .WithOne(r => r.Diagrama)
            .HasForeignKey<Relatorio>(o => o.IdDiagrama);
    }
}
