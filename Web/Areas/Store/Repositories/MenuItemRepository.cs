using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Repositories;
using Web.Data.Repositories.Store;
using Web.Models;

namespace Web.Areas.Store.Repositories
{
    public class MenuItemRepository(DefaultdbContext context) : Repository<MenuItem>(context), IMenuItemRepository
    {
    }
}