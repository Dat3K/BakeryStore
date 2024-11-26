using Auth0.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories;
using Web.Data.Repositories.Interfaces;
using Web.Services;
using Web.Config;
using Web.Services.Interfaces;
using Web.Areas.Store.Services.Interfaces;
using Web.Areas.Store.Services;
using Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();

// Configure Auth0
var auth0Settings = builder.Configuration.GetSection("Auth0").Get<Auth0Settings>();
builder.Services.AddSingleton(auth0Settings);

builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = auth0Settings.Domain;
    options.ClientId = auth0Settings.ClientId;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<DefaultdbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Store",
    pattern: "{area=Store}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "POS",
    pattern: "{area=POS}/{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
