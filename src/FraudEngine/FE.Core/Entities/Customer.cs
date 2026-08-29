namespace FE.Core.Entities
{
    public class Customer
    {
        public Guid Id { get; init; }
        public required string AccountNumber { get; init; }
        public required string Name { get; init; }
        public decimal AverageTransactionAmount { get; init; }
        public List<string> KnownCountries { get; init; } = [];
        public DateTimeOffset AccountCreatedAt { get; init; }
    }
}