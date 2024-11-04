namespace Web.Data.Repositories.Interfaces;
public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    T Get(int id);
    void Add(T entity);
    void Remove(int id);
    void Update(T entity);
}
