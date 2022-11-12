using Bogus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Customer.Api.Tests.Integration.CustomerEndpoint;

[Collection(nameof(SharedCustomerCollection))]
public class GetCustomerTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Func<Task> _resetDatabase;
    private readonly Faker<CustomerAPI.Models.Customer> _customerGenerator;

    public GetCustomerTests(CustomerApiFactory factory)
    {
        _client = factory.HttpClient;
        _resetDatabase = factory.ResetDatabase;
        _customerGenerator = CustomerApiFactory.CustomerGenerator;
    }

    [Fact]
    public async Task GetCustomers_ReturnsEmptyResult_WhenNoCustomerExists()
    {
        var response = await _client.GetAsync("/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerAPI.Models.Customer>>();

        customers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomers_ReturnsAllCustomers_WhenCustomerExist()
    {
        var existingCustomers = await AddCustomers();

        var response = await _client.GetAsync("/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerAPI.Models.Customer>>();

        customers.Should().BeEquivalentTo(existingCustomers);
    }

    [Fact]
    public async Task GetCustomers_ReturnsCustomerById_WhenCustomerExists()
    {
        var existingCustomers = await AddCustomers();

        var expectedCustomer = existingCustomers.First();

        var response = await _client.GetAsync($"/customers/{expectedCustomer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await response.Content.ReadFromJsonAsync<CustomerAPI.Models.Customer>();

        customer.Should().BeEquivalentTo(expectedCustomer);
    }

    [Fact]
    public async Task GetCustomers_ReturnsNotFound_WhenCustomerDoesntExist()
    {
        var response = await _client.GetAsync($"/customers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<List<CustomerAPI.Models.Customer>> AddCustomers(int numberOfCustomerToGenerate = 1)
    {
        var customers = _customerGenerator.Generate(numberOfCustomerToGenerate);

        foreach (var customer in customers)
        {
            await _client.PostAsJsonAsync("customers", customer);
        }

        return customers;
    }

    public Task DisposeAsync() => _resetDatabase();

    public Task InitializeAsync() => Task.CompletedTask;
}
