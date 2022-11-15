using Microsoft.EntityFrameworkCore;
using CustomerAPI.Data;
using CustomerAPI;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDbContext") ?? throw new InvalidOperationException("Connection string 'CustomerDbContext' not found."))
);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("customer_reader", policy => policy.RequireAssertion(context => context.User.IsInRole("customer") && (context.User.HasClaim(c => c.Type == "permission" && (c.Value == "read" || c.Value == "write")))))
    .AddPolicy("customer_writer", policy => policy.RequireAssertion(context => context.User.IsInRole("customer") && (context.User.HasClaim(c => c.Type == "permission" && c.Value == "write"))));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SwaggerGeneratorOptions>(opts => {
    opts.InferSecuritySchemes = true;
});


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