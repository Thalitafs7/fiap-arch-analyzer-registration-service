using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AnaliseRepository : IAnaliseRepository
{
    private readonly OrdensDbContext _context;

    public AnaliseRepository(OrdensDbContext context)
    {
        _context = context;
    }

    public async Task<Analise?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Analise>().FindAsync(new object[] { id }, cancellationToken);
    }


    public async Task AdicionarAsync(Analise entity, CancellationToken cancellationToken = default)
    {
        await _context.Anslise.AddAsync(entity, cancellationToken);
    }



    public async Task<IEnumerable<Analise>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Analise>().ToListAsync(cancellationToken);
    }


    public void Atualizar(Analise entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.Set<Analise>().Update(entity);
        }
        else
        {
            entry.State = EntityState.Modified;
        }
    }

    public void Remover(Analise entity)
    {
        _context.Set<Analise>().Remove(entity);
    }

    public void Deletar(Analise entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            entity.Ativo = false; // Marcar como inativo
            entity.DataAtualizacao = DateTime.UtcNow; // Atualizar a data de atualização            
            _context.Set<Analise>().Update(entity);
        }
        else
        {
            entry.State = EntityState.Modified;
        }
    }


    public async  Task<Analise> ObterPorOrdemServicoIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Analise>().FindAsync(new object[] { id }, cancellationToken);
    }
}
