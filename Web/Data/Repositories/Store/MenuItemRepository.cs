using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;

namespace Web.Data.Repositories
{
    public class MenuItemRepository(DefaultdbContext context) : Repository<MenuItem>(context), IMenuItemRepository
    {
    }
}
