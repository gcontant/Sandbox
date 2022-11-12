namespace Customer.Api.Tests.Integration;

[CollectionDefinition(nameof(SharedCustomerCollection))]
public class SharedCustomerCollection : ICollectionFixture<CustomerApiFactory>
{
}
