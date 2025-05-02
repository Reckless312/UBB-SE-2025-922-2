using ServerAPI.Controllers;
using ServerAPI.Data;
using Microsoft.EntityFrameworkCore;
using IRepository;
using Repository.AdminDashboard;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context to server
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer("Data Source=DESKTOP-KEB351O\\SQLEXPRESS;Initial Catalog=DrinkDB_Dev;Integrated Security=True;Trust Server Certificate=True"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.MapRoleEndpoints();

app.Run();
