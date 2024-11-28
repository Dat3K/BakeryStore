using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;

namespace Web.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DefaultdbContext _context;
    private IProductRepository? _productRepository;
    private IOrderRepository? _orderRepository;
    private ICategoryRepository? _categoryRepository;
    private IUserRepository? _userRepository;
    private bool _disposed;

    public UnitOfWork(DefaultdbContext context)
    {
        _context = context;
    }

    public IProductRepository ProductRepository =>
        _productRepository ??= new ProductRepository(_context);

    public IOrderRepository OrderRepository =>
        _orderRepository ??= new OrderRepository(_context);

    public ICategoryRepository CategoryRepository =>
        _categoryRepository ??= new CategoryRepository(_context);

    public IUserRepository UserRepository =>
        _userRepository ??= new UserRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
