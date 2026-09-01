using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Infrastructure.Rules;

namespace FE.UnitTests.Rules;

public class WatchlistRuleTests
{
    private readonly WatchlistRule _rule = new();

    private static Transaction CreateTransaction() => new()
    {
        AccountNumber = "ACC001",
        ReferenceId = "REF001",
        Amount = 100m,
        Currency = "USD",
        Country = "US",
        MerchantName = "Shady Corp",
        MerchantId = "MERCH001",
        PaymentChannel = PaymentChannel.Online,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static ScanSnapshot CreateSnapshot(WatchlistEntry? merchantEntry = null) => new()
    {
        Customer = new Customer { AccountNumber = "ACC001", Name = "Test" },
        MerchantWatchlistEntry = merchantEntry
    };

    [Fact]
    public void NoWatchlistEntry_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void InactiveWatchlistEntry_ReturnsClean()
    {
        var entry = new WatchlistEntry
        {
            EntityIdentifier = "MERCH001",
            Reason = "Fraud",
            ModifiedByIdentifier = "Test",
            IsActive = false,
            RiskLevel = Severity.High
        };

        var result = _rule.Evaluate(CreateTransaction(), CreateSnapshot(entry));
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void ActiveWatchlistEntry_TriggersWithEntrySeverity()
    {
        var entry = new WatchlistEntry
        {
            EntityIdentifier = "MERCH001",
            Reason = "Known fraud merchant",
            ModifiedByIdentifier = "Admin",
            IsActive = true,
            RiskLevel = Severity.Critical
        };

        var result = _rule.Evaluate(CreateTransaction(), CreateSnapshot(entry));

        Assert.True(result.IsTriggered);
        Assert.Equal("Watchlist", result.RuleName);
        Assert.Equal(Severity.Critical, result.Severity);
    }

    [Fact]
    public void ActiveWatchlistEntry_SeverityMatchesEntry()
    {
        var entry = new WatchlistEntry
        {
            EntityIdentifier = "MERCH001",
            Reason = "Suspicious activity",
            ModifiedByIdentifier = "Admin",
            IsActive = true,
            RiskLevel = Severity.Low
        };

        var result = _rule.Evaluate(CreateTransaction(), CreateSnapshot(entry));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Low, result.Severity);
    }

    [Fact]
    public void ApplicableChannels_IncludesAll()
    {
        Assert.Contains(PaymentChannel.CardPresent, _rule.ApplicableChannels);
        Assert.Contains(PaymentChannel.Online, _rule.ApplicableChannels);
        Assert.Contains(PaymentChannel.Transfer, _rule.ApplicableChannels);
    }
}
