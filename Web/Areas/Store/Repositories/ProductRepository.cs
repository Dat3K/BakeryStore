using System.Collections.Generic;
using System.Threading.Tasks;
using Web.Data.Repositories;
using Web.Data.Repositories.Store;
using Web.Models;

namespace Web.Areas.Store.Repositories
{
    public class ProductRepository(DefaultdbContext context) : Repository<Product>(context), IProductRepository
    {
    }
}