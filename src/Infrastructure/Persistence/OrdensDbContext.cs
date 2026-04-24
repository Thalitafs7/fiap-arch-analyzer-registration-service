using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public class OrdensDbContext : DbContext
{
    public DbSet<Diagrama> Diagrama => Set<Diagrama>();
    public DbSet<Analise> Anslise => Set<Analise>();
    public DbSet<Error> Error => Set<Error>();
    public DbSet<Relatorio> Relatorio => Set<Relatorio>();
    public OrdensDbContext(DbContextOptions<OrdensDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdensDbContext).Assembly);
    }
}
