using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Infrastructure.Rules;
using Microsoft.Extensions.Options;

namespace FE.UnitTests.Rules;

public class GeographicRuleTests
{
    private readonly GeographicRule _rule = new(Options.Create(
        new GeographicRuleOptions
        {
            MinTimeBetweenCountries = TimeSpan.FromHours(2)
        }));

    private static Transaction CreateTransaction(
        string country = "US", DateTimeOffset? createdAt = null) => new()
    {
        AccountNumber = "ACC001",
        ReferenceId = "REF001",
        Amount = 100m,
        Currency = "USD",
        Country = country,
        PaymentChannel = PaymentChannel.CardPresent,
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow
    };

    private static ScanSnapshot CreateSnapshot(List<Transaction>? recentTransactions = null) => new()
    {
        Customer = new Customer { AccountNumber = "ACC001", Name = "Test" },
        RecentTransactions = recentTransactions ?? []
    };

    [Fact]
    public void NoPreviousTransaction_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void SameCountry_ReturnsClean()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = CreateSnapshot([CreateTransaction("US", now.AddMinutes(-5))]);

        var result = _rule.Evaluate(CreateTransaction("US", now), snapshot);
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void DifferentCountry_OutsideMinTime_ReturnsClean()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = CreateSnapshot([CreateTransaction("US", now.AddHours(-3))]);

        var result = _rule.Evaluate(CreateTransaction("GB", now), snapshot);
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void DifferentCountry_Within5Minutes_ReturnsCritical()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = CreateSnapshot([CreateTransaction("US", now.AddMinutes(-3))]);

        var result = _rule.Evaluate(CreateTransaction("GB", now), snapshot);

        Assert.True(result.IsTriggered);
        Assert.Equal("Geographic", result.RuleName);
        Assert.Equal(Severity.Critical, result.Severity);
    }

    [Fact]
    public void DifferentCountry_Between5And30Minutes_ReturnsHigh()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = CreateSnapshot([CreateTransaction("US", now.AddMinutes(-20))]);

        var result = _rule.Evaluate(CreateTransaction("GB", now), snapshot);

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.High, result.Severity);
    }

    [Fact]
    public void DifferentCountry_Between30MinAnd2Hours_ReturnsMedium()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = CreateSnapshot([CreateTransaction("US", now.AddMinutes(-60))]);

        var result = _rule.Evaluate(CreateTransaction("GB", now), snapshot);

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void UsesLastTransactionFromSnapshot()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = CreateSnapshot([
            CreateTransaction("GB", now.AddHours(-5)),
            CreateTransaction("US", now.AddMinutes(-3))
        ]);

        var result = _rule.Evaluate(CreateTransaction("FR", now), snapshot);

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Critical, result.Severity);
    }

    [Fact]
    public void OnlyAppliesToCardPresent()
    {
        Assert.Single(_rule.ApplicableChannels);
        Assert.Contains(PaymentChannel.CardPresent, _rule.ApplicableChannels);
    }
}
