using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class RelatorioRepository : IRelatorioRepository
{
    private readonly OrdensDbContext _context;

    public RelatorioRepository(OrdensDbContext context)
    {
        _context = context;
    }

    public async Task<Relatorio?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Relatorio>().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Relatorio> ObterPorOrdemServicoIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Relatorio>().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Relatorio>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Relatorio>().ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Relatorio entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<Relatorio>().AddAsync(entity, cancellationToken);
    }

    public void Atualizar(Relatorio entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.Set<Relatorio>().Update(entity);
        }
        else
        {
            entry.State = EntityState.Modified;
        }
    }

    public void Remover(Relatorio entity)
    {
        _context.Set<Relatorio>().Remove(entity);
    }

    public void Deletar(Relatorio entity)
    {
        throw new NotImplementedException();
    }
}
