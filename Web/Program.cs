using Auth0.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Store.Repositories;
using Web.Areas.Store.Services;
using Web.Data.Repositories.Store;
using Web.Models;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddDbContext<DefaultdbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add Auth0
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
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
app.UseAuthorization();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Store",
    pattern: "{area=Store}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "POS",
    pattern: "{area=POS}/{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
