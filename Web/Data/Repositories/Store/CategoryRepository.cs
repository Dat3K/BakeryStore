using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;

namespace Web.Data.Repositories
{
    public class CategoryRepository(DefaultdbContext context) : Repository<Category>(context), ICategoryRepository
    {
    }
}
