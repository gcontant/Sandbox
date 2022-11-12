using Bogus;
using FluentAssertions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;
using System.Net.Http.Json;

namespace Customer.Api.Tests.Integration.CustomerEndpoint;

[Collection(nameof(SharedCustomerCollection))]
public class UpdateCustomerTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly Func<Task> _resetDatabase;
    private readonly Faker<CustomerAPI.Models.Customer> _customerGenerator;

    public UpdateCustomerTests(CustomerApiFactory factory)
    {
        _httpClient = factory.HttpClient;
        _resetDatabase = factory.ResetDatabase;
        _customerGenerator = CustomerApiFactory.CustomerGenerator;
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsNoContent_WhenCustomerExists()
    {
        var customer = _customerGenerator.Generate();
        _ = await _httpClient.PostAsJsonAsync("customers", customer);

        customer.FirstName = "Test";
        
        var response = await _httpClient.PutAsJsonAsync($"customers/{customer.Id}", customer);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var retrievededCustomer = await _httpClient.GetAsync($"customers/{customer.Id}");
        var actualCustomer = await retrievededCustomer.Content.ReadFromJsonAsync<CustomerAPI.Models.Customer>();

        actualCustomer!.FirstName.Should().Be("Test");
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerDoesntExist()
    {
        var customer = _customerGenerator.Generate();
        var response = await _httpClient.PutAsJsonAsync($"customers/{Guid.NewGuid()}", customer);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public Task DisposeAsync() => _resetDatabase();

    public Task InitializeAsync() => Task.CompletedTask;
}
