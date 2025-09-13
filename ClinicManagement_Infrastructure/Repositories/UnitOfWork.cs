// unitofwork

using ClinicManagement_Infrastructure.Infrastructure.Data;

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T> Repository<T>()
        where T : class;
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly SupabaseContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(SupabaseContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>()
        where T : class
    {
        if (!_repositories.ContainsKey(typeof(T)))
        {
            var repositoryInstance = new Repository<T>(_context);
            _repositories[typeof(T)] = repositoryInstance;
        }
        return (IRepository<T>)_repositories[typeof(T)];
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
