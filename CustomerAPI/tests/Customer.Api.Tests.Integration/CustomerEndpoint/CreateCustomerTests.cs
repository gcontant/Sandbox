using Bogus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Customer.Api.Tests.Integration.CustomerEndpoint;

[Collection(nameof(SharedCustomerCollection))]
public class CreateCustomerTests : IAsyncLifetime
{
    private readonly Faker<CustomerAPI.Models.Customer> _customerGenerator;
    private readonly HttpClient _client;

    private readonly Func<Task> _resetDatabase;

    public CreateCustomerTests(CustomerApiFactory factory)
    {
        _client = factory.HttpClient;
        _resetDatabase = factory.ResetDatabase;
        _customerGenerator = CustomerApiFactory.CustomerGenerator;
    }

    [Fact]
    public async void Create_CreatesCustomer_WhenDataIsValid()
    {
        var customer = _customerGenerator.Generate();

        var response = await _client.PostAsJsonAsync("customers", customer);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<CustomerAPI.Models.Customer>();
        content.Should().BeEquivalentTo(customer);

        response.Headers.Location!.ToString().Should().Be($"/api/Customer/{content!.Id}");
    }

    public Task DisposeAsync() => _resetDatabase();

    public Task InitializeAsync() => Task.CompletedTask;
}