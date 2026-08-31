using FE.Core.Entities;
using FE.Core.Enums;

namespace FE.Core.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByAccountNumber(string accountNumber, CancellationToken ct);
        Task Add(Customer customer, CancellationToken ct);
        Task Add(CustomerChannelAverage customerChannelAverage, CancellationToken ct);
        void UpdateCustomer(Customer customer);
        void UpdateCustomerAverage(CustomerChannelAverage customerChannelAverage);
        Task<CustomerChannelAverage?> GetByCustomerAndChannel(Guid customerId, PaymentChannel channel, CancellationToken ct);
    }
}
