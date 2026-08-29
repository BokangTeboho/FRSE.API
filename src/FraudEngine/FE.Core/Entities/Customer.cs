namespace FE.Core.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public IList<string> KnownCountries { get; set; } = [];
        public DateTimeOffset AccountCreatedAt { get; set; }
    }
}
