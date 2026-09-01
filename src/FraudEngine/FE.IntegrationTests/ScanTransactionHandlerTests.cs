using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Features.Transaction.ScanTransaction;
using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using FE.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FE.IntegrationTests;

public class ScanTransactionHandlerTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public ScanTransactionHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private static ScanTransactionCommandHandler CreateHandler(IServiceScope scope)
    {
        var sp = scope.ServiceProvider;
        return new ScanTransactionCommandHandler(
            sp.GetRequiredService<ICustomerRepository>(),
            sp.GetRequiredService<ITransactionRepository>(),
            sp.GetRequiredService<IFraudAlertRepository>(),
            sp.GetRequiredService<IWatchlistService>(),
            sp.GetRequiredService<IEnumerable<IFraudRule>>(),
            sp.GetRequiredService<IUnitOfWork>(),
            sp.GetRequiredService<ILogger<ScanTransactionCommandHandler>>());
    }

    private static ScanTransactionCommand CreateCommand(
        string? referenceId = null,
        string? accountNumber = null,
        decimal amount = 500m,
        string currency = "USD",
        string country = "US",
        PaymentChannel channel = PaymentChannel.Online,
        string? merchantId = null,
        string? merchantName = null,
        string? beneficiaryAccount = null)
    {
        return new ScanTransactionCommand
        {
            ReferenceId = referenceId ?? Guid.NewGuid().ToString(),
            AccountNumber = accountNumber ?? $"ACC-{Guid.NewGuid():N}",
            CustomerName = "Test Customer",
            Amount = amount,
            Currency = currency,
            Country = country,
            PaymentChannel = channel,
            PaymentTiming = PaymentTiming.Immediate,
            MerchantId = merchantId,
            MerchantName = merchantName,
            BeneficiaryAccountNumber = beneficiaryAccount
        };
    }

    [Fact]
    public async Task NewTransaction_BelowThreshold_NotFlagged()
    {
        using var scope = _fixture.CreateScope();
        var handler = CreateHandler(scope);
        var command = CreateCommand(amount: 500m);

        var result = await handler.ExecuteAsync(command, CancellationToken.None);

        Assert.Empty(result.TriggeredRules);

        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var tx = await db.Transactions.FirstAsync(t => t.ReferenceId == command.ReferenceId);
        Assert.False(tx.IsFlagged);
    }

    [Fact]
    public async Task NewTransaction_AboveThreshold_FlaggedWithAlert()
    {
        using var scope = _fixture.CreateScope();
        var handler = CreateHandler(scope);
        var command = CreateCommand(amount: 15_000m);

        var result = await handler.ExecuteAsync(command, CancellationToken.None);

        Assert.NotEmpty(result.TriggeredRules);
        Assert.Contains(result.TriggeredRules, r => r.RuleName == "Threshold");

        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var tx = await db.Transactions.FirstAsync(t => t.ReferenceId == command.ReferenceId);
        Assert.True(tx.IsFlagged);

        var alerts = await db.FraudAlerts.Where(a => a.TransactionId == tx.Id).ToListAsync();
        Assert.Contains(alerts, a => a.RuleName == "Threshold");
    }

    [Fact]
    public async Task DuplicateTransaction_ReturnsExistingResult()
    {
        using var scope = _fixture.CreateScope();
        var handler = CreateHandler(scope);
        var referenceId = Guid.NewGuid().ToString();
        var accountNumber = $"ACC-{Guid.NewGuid():N}";
        var command = CreateCommand(
            referenceId: referenceId, accountNumber: accountNumber, amount: 15_000m);

        var firstResult = await handler.ExecuteAsync(command, CancellationToken.None);
        var secondResult = await handler.ExecuteAsync(command, CancellationToken.None);

        Assert.Equal(firstResult.ReferenceId, secondResult.ReferenceId);
        Assert.Equal(firstResult.TriggeredRules.Count, secondResult.TriggeredRules.Count);

        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var txCount = await db.Transactions
            .CountAsync(t => t.ReferenceId == referenceId && t.AccountNumber == accountNumber);
        Assert.Equal(1, txCount);
    }

    [Fact]
    public async Task NewCustomer_CreatedAutomatically()
    {
        using var scope = _fixture.CreateScope();
        var handler = CreateHandler(scope);
        var accountNumber = $"ACC-{Guid.NewGuid():N}";
        var command = CreateCommand(accountNumber: accountNumber, country: "ZA");

        await handler.ExecuteAsync(command, CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

        Assert.NotNull(customer);
        Assert.Equal("Test Customer", customer.Name);
        Assert.Contains("ZA", customer.KnownCountries);
    }

    [Fact]
    public async Task ExistingCustomer_NewCountryAdded()
    {
        using var scope = _fixture.CreateScope();
        var handler = CreateHandler(scope);
        var accountNumber = $"ACC-{Guid.NewGuid():N}";

        await handler.ExecuteAsync(
            CreateCommand(accountNumber: accountNumber, country: "US"),
            CancellationToken.None);

        await handler.ExecuteAsync(
            CreateCommand(accountNumber: accountNumber, country: "GB"),
            CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var customer = await db.Customers.FirstAsync(c => c.AccountNumber == accountNumber);
        Assert.Contains("US", customer.KnownCountries);
        Assert.Contains("GB", customer.KnownCountries);
    }

    [Fact]
    public async Task ChannelAverage_CreatedForNewCustomer()
    {
        using var scope = _fixture.CreateScope();
        var handler = CreateHandler(scope);
        var accountNumber = $"ACC-{Guid.NewGuid():N}";

        await handler.ExecuteAsync(
            CreateCommand(accountNumber: accountNumber, amount: 1000m),
            CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var customer = await db.Customers.FirstAsync(c => c.AccountNumber == accountNumber);
        var average = await db.CustomerChannelAverages
            .FirstOrDefaultAsync(a =>
                a.CustomerId == customer.Id
                && a.PaymentChannel == PaymentChannel.Online);

        Assert.NotNull(average);
        Assert.Equal(1000m, average.AverageAmount);
        Assert.Equal(1, average.TransactionCount);
    }

    [Fact]
    public async Task ChannelAverage_UpdatedIncrementally()
    {
        using var scope = _fixture.CreateScope();
        var handler = CreateHandler(scope);
        var accountNumber = $"ACC-{Guid.NewGuid():N}";

        await handler.ExecuteAsync(
            CreateCommand(accountNumber: accountNumber, amount: 1000m),
            CancellationToken.None);
        await handler.ExecuteAsync(
            CreateCommand(accountNumber: accountNumber, amount: 3000m),
            CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var customer = await db.Customers.FirstAsync(c => c.AccountNumber == accountNumber);
        var average = await db.CustomerChannelAverages
            .FirstAsync(a =>
                a.CustomerId == customer.Id
                && a.PaymentChannel == PaymentChannel.Online);

        Assert.Equal(2000m, average.AverageAmount);
        Assert.Equal(2, average.TransactionCount);
    }

    [Fact]
    public async Task WatchlistedMerchant_TriggersAlert()
    {
        using var scope = _fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var merchantId = $"MERCH-{Guid.NewGuid():N}";

        db.WatchlistEntries.Add(new WatchlistEntry
        {
            Id = Guid.NewGuid(),
            EntityType = EntityType.Merchant,
            EntityIdentifier = merchantId,
            RiskLevel = Severity.High,
            Reason = "Known fraud",
            ModifiedByIdentifier = "Test",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var command = CreateCommand(
            merchantId: merchantId, merchantName: "Shady Corp", amount: 100m);

        var result = await handler.ExecuteAsync(command, CancellationToken.None);

        Assert.Contains(result.TriggeredRules, r => r.RuleName == "Watchlist");
    }

    [Fact]
    public async Task MultipleRulesTriggered_AllAlertsSaved()
    {
        using var scope = _fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var merchantId = $"MERCH-{Guid.NewGuid():N}";

        db.WatchlistEntries.Add(new WatchlistEntry
        {
            Id = Guid.NewGuid(),
            EntityType = EntityType.Merchant,
            EntityIdentifier = merchantId,
            RiskLevel = Severity.High,
            Reason = "Fraud",
            ModifiedByIdentifier = "Test",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var command = CreateCommand(
            amount: 15_000m,
            merchantId: merchantId,
            merchantName: "Bad Corp");

        var result = await handler.ExecuteAsync(command, CancellationToken.None);

        Assert.True(result.TriggeredRules.Count >= 2);
        Assert.Contains(result.TriggeredRules, r => r.RuleName == "Threshold");
        Assert.Contains(result.TriggeredRules, r => r.RuleName == "Watchlist");

        var tx = await db.Transactions.FirstAsync(t => t.ReferenceId == command.ReferenceId);
        var alerts = await db.FraudAlerts
            .Where(a => a.TransactionId == tx.Id).ToListAsync();
        Assert.True(alerts.Count >= 2);
    }

    [Fact]
    public async Task TransactionFields_StoredUpperCase()
    {
        using var scope = _fixture.CreateScope();
        var handler = CreateHandler(scope);
        var command = CreateCommand(currency: "usd", country: "us");

        await handler.ExecuteAsync(command, CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        var tx = await db.Transactions.FirstAsync(t => t.ReferenceId == command.ReferenceId);
        Assert.Equal("USD", tx.Currency);
        Assert.Equal("US", tx.Country);
    }
}
