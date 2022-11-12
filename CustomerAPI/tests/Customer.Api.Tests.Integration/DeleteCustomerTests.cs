using Bogus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Customer.Api.Tests.Integration;

[Collection(nameof(SharedCustomerCollection))]
public class DeleteCustomerTests : IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    private readonly HttpClient _httpClient;
    private readonly Faker<CustomerAPI.Models.Customer> _customerGenerator;

    public DeleteCustomerTests(CustomerApiFactory factory)
    {
        _resetDatabase = factory.ResetDatabase;
        _httpClient = factory.HttpClient;
        _customerGenerator = CustomerApiFactory.CustomerGenerator;
    }

    [Fact]
    public async Task Delete_DeletesCustomer_WhenCustomerExists()
    {
        var customer = _customerGenerator.Generate();
        _ = await _httpClient.PostAsJsonAsync("customers", customer);

        var response = await _httpClient.DeleteAsync($"customers/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response = await _httpClient.GetAsync($"customers/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ReturnNotFound_WhenCustomerDoesntExist()
    {
        var response = await _httpClient.DeleteAsync($"customers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    public Task DisposeAsync() => _resetDatabase();
    public Task InitializeAsync() => Task.CompletedTask;
}
