using Microsoft.EntityFrameworkCore;
using CustomerAPI.Data;
using CustomerAPI;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDbContext") ?? throw new InvalidOperationException("Connection string 'CustomerDbContext' not found."));
    options.EnableSensitiveDataLogging();
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapCustomerEndpoints();

app.Run();

public partial class Program { }
