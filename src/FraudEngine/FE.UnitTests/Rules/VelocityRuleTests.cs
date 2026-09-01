using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Infrastructure.Rules;
using Microsoft.Extensions.Options;

namespace FE.UnitTests.Rules;

public class VelocityRuleTests
{
    private readonly VelocityRule _rule = new(Options.Create(
        new VelocityRuleOptions
        {
            Window = TimeSpan.FromMinutes(10),
            MaxTransactions = 5
        }));

    private static Transaction CreateTransaction(DateTimeOffset? createdAt = null) => new()
    {
        AccountNumber = "ACC001",
        ReferenceId = "REF001",
        Amount = 100m,
        Currency = "USD",
        Country = "US",
        PaymentChannel = PaymentChannel.Online,
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow
    };

    private static ScanSnapshot CreateSnapshot(List<Transaction>? recentTransactions = null) => new()
    {
        Customer = new Customer { AccountNumber = "ACC001", Name = "Test" },
        RecentTransactions = recentTransactions ?? []
    };

    [Fact]
    public void NoRecentTransactions_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void BelowMaxTransactions_ReturnsClean()
    {
        var now = DateTimeOffset.UtcNow;
        var recent = Enumerable.Range(0, 4)
            .Select(i => CreateTransaction(now.AddMinutes(-i)))
            .ToList();

        var result = _rule.Evaluate(CreateTransaction(now), CreateSnapshot(recent));
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void AtMaxTransactions_ReturnsMedium()
    {
        var now = DateTimeOffset.UtcNow;
        var recent = Enumerable.Range(0, 5)
            .Select(i => CreateTransaction(now.AddMinutes(-i)))
            .ToList();

        var result = _rule.Evaluate(CreateTransaction(now), CreateSnapshot(recent));

        Assert.True(result.IsTriggered);
        Assert.Equal("Velocity", result.RuleName);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void DoubleMaxTransactions_ReturnsHigh()
    {
        var now = DateTimeOffset.UtcNow;
        var recent = Enumerable.Range(0, 10)
            .Select(i => CreateTransaction(now.AddSeconds(-i * 30)))
            .ToList();

        var result = _rule.Evaluate(CreateTransaction(now), CreateSnapshot(recent));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.High, result.Severity);
    }

    [Fact]
    public void TripleMaxTransactions_ReturnsCritical()
    {
        var now = DateTimeOffset.UtcNow;
        var recent = Enumerable.Range(0, 15)
            .Select(i => CreateTransaction(now.AddSeconds(-i * 30)))
            .ToList();

        var result = _rule.Evaluate(CreateTransaction(now), CreateSnapshot(recent));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Critical, result.Severity);
    }

    [Fact]
    public void TransactionsOutsideWindow_NotCounted()
    {
        var now = DateTimeOffset.UtcNow;
        var recent = Enumerable.Range(0, 10)
            .Select(i => CreateTransaction(now.AddMinutes(-15 - i)))
            .ToList();

        var result = _rule.Evaluate(CreateTransaction(now), CreateSnapshot(recent));
        Assert.False(result.IsTriggered);
    }
}
