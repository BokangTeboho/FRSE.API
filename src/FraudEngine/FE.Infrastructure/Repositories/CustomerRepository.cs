using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FE.Infrastructure.Repositories
{
    public class CustomerRepository(FraudEngineDbContext db) : ICustomerRepository
    {
        public async Task Add(Customer customer, CancellationToken ct)
        {
            await db.Customers.AddAsync(customer, ct);
        }

        public async Task Add(CustomerChannelAverage customerChannelAverage, CancellationToken ct)
        {
            await db.CustomerChannelAverages.AddAsync(customerChannelAverage, ct);
        }

        public async Task<Customer?> GetByAccountNumber(string accountNumber, CancellationToken ct)
        {
            return await db.Customers
                .Where(c => c.AccountNumber == accountNumber)
                .FirstOrDefaultAsync(ct);
        }

        public Task<CustomerChannelAverage?> GetByCustomerAndChannel(Guid customerId, PaymentChannel channel, CancellationToken ct)
        {
            return db.CustomerChannelAverages
                .Where(c => c.CustomerId == customerId && c.PaymentChannel == channel)
                .FirstOrDefaultAsync(ct);
        }
    }
}
