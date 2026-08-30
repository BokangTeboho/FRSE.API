using FastEndpoints;
using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FE.Core.Features.Transaction.ScanTransaction
{
    public record ScanTransactionCommand : ICommand<ScanTransactionResult>
    {
        public required string ReferenceId { get; init; }
        public required string AccountNumber { get; init; }
        public required string CustomerName { get; init; }
        public required decimal Amount { get; init; }
        public required string Currency { get; init; }
        public required string Country { get; init; }
        public required PaymentChannel PaymentChannel { get; init; }
        public required PaymentTiming PaymentTiming { get; init; }
        public string? MerchantName { get; init; }
        public string? MerchantId { get; init; }
        public string? BeneficiaryAccountNumber { get; init; }
        public string? Category { get; init; }
    }

    public record ScanTransactionResult(string ReferenceId, IList<FraudRuleResult> TriggeredRules);

    public class ScanTransactionCommandHandler(
        ICustomerRepository customerRepository,
        ITransactionRepository transactionRepository,
        IFraudAlertRepository fraudAlertRepository,
        IWatchlistService watchlistService,
        IEnumerable<IFraudRule> rules,
        IUnitOfWork unitOfWork,
        ILogger<ScanTransactionCommandHandler> logger) : ICommandHandler<ScanTransactionCommand, ScanTransactionResult>
    {
        public async Task<ScanTransactionResult> ExecuteAsync(ScanTransactionCommand command, CancellationToken ct)
        {
            var customer = await GetOrCreateCustomer(command, ct);

            var recentTransactions = await transactionRepository
                .GetRecentByAccountNumber(command.AccountNumber, TimeSpan.FromHours(24), ct);

            var merchantWatchlistEntry = command.MerchantId is not null
                ? await watchlistService.CheckMerchant(command.MerchantId, ct)
                : null;

            var beneficiaryWatchlistEntry = command.BeneficiaryAccountNumber is not null
                ? await watchlistService.CheckBeneficiary(command.BeneficiaryAccountNumber, ct)
                : null;

            var snapshot = new ScanSnapshot
            {
                Customer = customer,
                RecentTransactions = recentTransactions,
                MerchantWatchlistEntry = merchantWatchlistEntry,
                BeneficiaryWatchlistEntry = beneficiaryWatchlistEntry
            };

            var transaction = new Entities.Transaction
            {
                Id = Guid.NewGuid(),
                AccountNumber = command.AccountNumber,
                Amount = command.Amount,
                Currency = command.Currency,
                ReferenceId = command.ReferenceId,
                Country = command.Country,
                MerchantId = command.MerchantId,
                MerchantName = command.MerchantName,
                BeneficiaryAccountNumber = command.BeneficiaryAccountNumber,
                Category = command.Category,
                PaymentChannel = command.PaymentChannel,
                PaymentTiming = command.PaymentTiming,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var triggeredRules = rules
                .Where(r => r.ApplicableChannels.Contains(command.PaymentChannel))
                .Select(r => r.Evaluate(transaction, snapshot))
                .Where(r => r.IsTriggered)
                .ToList();

            transaction.IsFlagged = triggeredRules.Any();

            await transactionRepository.Add(transaction, ct);

            foreach (var result in triggeredRules)
            {
                logger.LogInformation(
                    "Fraud alert triggered: Rule={RuleName}, Severity={Severity}, TransactionId={TransactionId}",
                    result.RuleName, result.Severity.ToString(), transaction.Id);

                await fraudAlertRepository.Add(new FraudAlert
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transaction.Id,
                    RuleName = result.RuleName!,
                    Severity = result.Severity!.Value,
                    Description = result.Description!,
                    CreatedAt = DateTimeOffset.UtcNow
                }, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);

            return new ScanTransactionResult(command.ReferenceId, triggeredRules);
        }

        private async Task<Customer> GetOrCreateCustomer(ScanTransactionCommand command, CancellationToken ct)
        {
            var customer = await customerRepository.GetByAccountNumber(command.AccountNumber, ct);

            if (customer is not null)
                return customer;

            customer = new Customer
            {
                Id = Guid.NewGuid(),
                AccountNumber = command.AccountNumber,
                Name = command.CustomerName,
                AverageTransactionAmount = 0,
                KnownCountries = [],
                AccountCreatedAt = DateTimeOffset.UtcNow
            };

            await customerRepository.Add(customer, ct);

            return customer;
        }
    }
}
