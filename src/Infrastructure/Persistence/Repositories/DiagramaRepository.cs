using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DiagramaRepository : IDiagramaRepository
{
    private readonly OrdensDbContext _context;

    public DiagramaRepository(OrdensDbContext context)
    {
        _context = context;
    }


    public async Task<Diagrama?> ObterPorAnaliseAsync(Guid analiseId, CancellationToken cancellationToken = default)
    {
        return await _context.Diagrama.Where(o => o.AnaliseId == analiseId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Diagrama?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Diagrama.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Diagrama?> ObterComDetalhesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Diagrama
            //.Include(o => o.Insumos)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Diagrama>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Diagrama
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Diagrama>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _context.Diagrama
            /*.Include(o => o.Orcamento)
            .Include(o => o.Pagamento)
            .Include(o => o.Insumos)*/
            .Where(o => o.Id == clienteId)
            .OrderByDescending(o => o.DataCadastro)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Diagrama entity, CancellationToken cancellationToken = default)
    {
        await _context.Diagrama.AddAsync(entity, cancellationToken);
    }

    public void Atualizar(Diagrama entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.Diagrama.Update(entity);
        }
        else
        {
            entry.State = EntityState.Modified;
        }
    }

    public void Remover(Diagrama entity)
    {
        _context.Diagrama.Remove(entity);
    }

    public void Deletar(Diagrama entity)
    {
        throw new NotImplementedException();
    }
}
