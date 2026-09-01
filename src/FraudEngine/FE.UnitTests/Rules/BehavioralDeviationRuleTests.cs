using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Infrastructure.Rules;
using Microsoft.Extensions.Options;

namespace FE.UnitTests.Rules;

public class BehavioralDeviationRuleTests
{
    private readonly BehavioralDeviationRule _rule = new(Options.Create(
        new BehavioralDeviationRuleOptions { DeviationMultiplier = 3.0m }));

    private static Transaction CreateTransaction(decimal amount = 100m) => new()
    {
        AccountNumber = "ACC001",
        ReferenceId = "REF001",
        Amount = amount,
        Currency = "USD",
        Country = "US",
        PaymentChannel = PaymentChannel.Online,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static ScanSnapshot CreateSnapshot(decimal averageAmount, int transactionCount = 10) => new()
    {
        Customer = new Customer { AccountNumber = "ACC001", Name = "Test" },
        ChannelAverage = new CustomerChannelAverage
        {
            AverageAmount = averageAmount,
            TransactionCount = transactionCount
        }
    };

    [Fact]
    public void AverageIsZero_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(1000m), CreateSnapshot(0m, 0));
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void AmountBelowMultiplier_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(200m), CreateSnapshot(100m));
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void AmountAtMultiplier_ReturnsMedium()
    {
        var result = _rule.Evaluate(CreateTransaction(300m), CreateSnapshot(100m));

        Assert.True(result.IsTriggered);
        Assert.Equal("BehavioralDeviation", result.RuleName);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void AmountBetween3xAnd5x_ReturnsMedium()
    {
        var result = _rule.Evaluate(CreateTransaction(400m), CreateSnapshot(100m));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void Amount5xAverage_ReturnsHigh()
    {
        var result = _rule.Evaluate(CreateTransaction(500m), CreateSnapshot(100m));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.High, result.Severity);
    }

    [Fact]
    public void AmountBetween5xAnd10x_ReturnsHigh()
    {
        var result = _rule.Evaluate(CreateTransaction(700m), CreateSnapshot(100m));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.High, result.Severity);
    }

    [Fact]
    public void Amount10xAverage_ReturnsCritical()
    {
        var result = _rule.Evaluate(CreateTransaction(1000m), CreateSnapshot(100m));

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Critical, result.Severity);
    }

    [Fact]
    public void ApplicableChannels_IncludesAll()
    {
        Assert.Contains(PaymentChannel.CardPresent, _rule.ApplicableChannels);
        Assert.Contains(PaymentChannel.Online, _rule.ApplicableChannels);
        Assert.Contains(PaymentChannel.Transfer, _rule.ApplicableChannels);
    }
}
