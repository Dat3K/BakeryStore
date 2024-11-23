using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;

namespace Web.Data.Repositories
{
    public class ProductRepository(DefaultdbContext context) : Repository<Product>(context), IProductRepository
    {
    }
}
