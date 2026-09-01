using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Infrastructure.Rules;
using Microsoft.Extensions.Options;

namespace FE.UnitTests.Rules;

public class StructuringRuleTests
{
    private readonly StructuringRule _rule = new(Options.Create(
        new StructuringRuleOptions
        {
            Thresholds = new() { ["USD"] = 10_000m },
            ProximityPercentage = 0.1m
        }));

    private static Transaction CreateTransaction(
        decimal amount, string currency = "USD") => new()
    {
        AccountNumber = "ACC001",
        ReferenceId = "REF001",
        Amount = amount,
        Currency = currency,
        Country = "US",
        PaymentChannel = PaymentChannel.Online,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static ScanSnapshot CreateSnapshot(List<Transaction>? recentTransactions = null) => new()
    {
        Customer = new Customer { AccountNumber = "ACC001", Name = "Test" },
        RecentTransactions = recentTransactions ?? []
    };

    [Fact]
    public void AmountWellBelowThreshold_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(5000m), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void AmountAboveThreshold_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(11_000m), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void AmountAtThreshold_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(10_000m), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void CurrencyNotInThresholds_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(9500m, "EUR"), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void NearThreshold_NoRecentSimilar_ReturnsLow()
    {
        var result = _rule.Evaluate(CreateTransaction(9500m), CreateSnapshot());

        Assert.True(result.IsTriggered);
        Assert.Equal("Structuring", result.RuleName);
        Assert.Equal(Severity.Low, result.Severity);
    }

    [Fact]
    public void NearThreshold_OneRecentSimilar_ReturnsMedium()
    {
        var recent = new List<Transaction> { CreateTransaction(9200m) };

        var result = _rule.Evaluate(CreateTransaction(9500m), CreateSnapshot(recent));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void NearThreshold_TwoRecentSimilar_ReturnsHigh()
    {
        var recent = new List<Transaction>
        {
            CreateTransaction(9200m),
            CreateTransaction(9300m)
        };

        var result = _rule.Evaluate(CreateTransaction(9500m), CreateSnapshot(recent));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.High, result.Severity);
    }

    [Fact]
    public void NearThreshold_ThreeOrMoreRecentSimilar_ReturnsCritical()
    {
        var recent = new List<Transaction>
        {
            CreateTransaction(9100m),
            CreateTransaction(9200m),
            CreateTransaction(9300m)
        };

        var result = _rule.Evaluate(CreateTransaction(9500m), CreateSnapshot(recent));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Critical, result.Severity);
    }

    [Fact]
    public void NearThreshold_RecentOutsideProximity_NotCounted()
    {
        var recent = new List<Transaction> { CreateTransaction(5000m) };

        var result = _rule.Evaluate(CreateTransaction(9500m), CreateSnapshot(recent));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Low, result.Severity);
    }
}
