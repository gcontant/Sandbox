﻿using Microsoft.EntityFrameworkCore;
using CustomerAPI.Data;
using CustomerAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using CustomerAPI.EndpointFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace CustomerAPI;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder routes)
    {


        var group = routes.MapGroup("/customers")
                .WithTags(nameof(Customer))
                .AddEndpointFilter<RequestAuditorFilter>()
                .AddEndpointFilterFactory((context, next) =>
                {
                    var loggerFactory = context.ApplicationServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("PostRequestAuditor");
                    return async (invocationContext) =>
                    {
                        var result = await next(invocationContext);
                        logger.LogInformation($"[⚙️] Result from request: {result}");
                        return result;
                    };
                })
                .EnableOpenApiWithAuthentication();

        group.MapGet("/", async (CustomerDbContext db) =>
        {
            return await db.Customer.ToListAsync();
        })
        .WithName("GetAllCustomers")
        .RequireAuthorization("customer_reader");

        group.MapGet("/{id}", async Task<Results<Ok<Customer>, NotFound>> (Guid id, CustomerDbContext db) =>
        {
            return await db.Customer.FindAsync(id)
                is Customer model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetCustomerById")
        .RequireAuthorization("customer_reader");

        group.MapPut("/{id}", async Task<Results<NotFound, NoContent>> (Guid id, Customer updatedCustomer, CustomerDbContext db) =>
        {
            var existingCustomer = await db.Customer.FindAsync(id);

            if (existingCustomer is null)
            {
                return TypedResults.NotFound();
            }

            existingCustomer.FirstName = updatedCustomer.FirstName;
            existingCustomer.LastName = updatedCustomer.LastName;

            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        })
        .WithName("UpdateCustomer")
        .RequireAuthorization("customer_writer");

        group.MapPost("/", async (Customer customer, CustomerDbContext db) =>
        {
            db.Customer.Add(customer);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Customer/{customer.Id}", customer);
        })
        .WithName("CreateCustomer")
        .RequireAuthorization("customer_writer");

        group.MapDelete("/{id}", async Task<Results<Ok<Customer>, NotFound>> (Guid id, CustomerDbContext db) =>
        {
            if (await db.Customer.FindAsync(id) is Customer customer)
            {
                db.Customer.Remove(customer);
                await db.SaveChangesAsync();
                return TypedResults.Ok(customer);
            }

            return TypedResults.NotFound();
        })
        .WithName("DeleteCustomer")
        .RequireAuthorization("customer_writer");
    }
}
