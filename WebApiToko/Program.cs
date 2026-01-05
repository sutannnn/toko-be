using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApiToko.ModelsEF.Toko;

var builder = WebApplication.CreateBuilder(args);

// Konfigurasi DBContext ke SQL Server/LocalDB
builder.Services.AddDbContext<TokoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Tambahkan Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<TokoDbContext>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
