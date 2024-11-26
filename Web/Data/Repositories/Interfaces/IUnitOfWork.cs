namespace Web.Data.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICartRepository CartRepository { get; }
    IProductRepository ProductRepository { get; }
    IOrderRepository OrderRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IUserRepository UserRepository { get; }
    Task<int> SaveChangesAsync();
}
