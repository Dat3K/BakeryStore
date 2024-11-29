namespace Web.Data.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository ProductRepository { get; }
    IOrderRepository OrderRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IUserRepository UserRepository { get; }
    IOrderItemRepository OrderItemRepository { get; }
    Task<int> SaveChangesAsync();
}
