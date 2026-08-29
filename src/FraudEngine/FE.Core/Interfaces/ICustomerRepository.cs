using FE.Core.Entities;

namespace FE.Core.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByAccountNumber(string accountNumber, CancellationToken ct);
        Task Add(Customer customer, CancellationToken ct);
    }
}
