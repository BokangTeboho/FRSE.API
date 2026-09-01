using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Infrastructure.Rules;
using Microsoft.Extensions.Options;

namespace FE.UnitTests.Rules;

public class ThresholdRuleTests
{
    private readonly ThresholdRule _rule = new(Options.Create(
        new ThresholdRuleOptions
        {
            DefaultLimit = 10_000m,
            Limits = new() { ["USD"] = 10_000m, ["ZAR"] = 150_000m }
        }));

    private static Transaction CreateTransaction(decimal amount, string currency = "USD") => new()
    {
        AccountNumber = "ACC001",
        ReferenceId = "REF001",
        Amount = amount,
        Currency = currency,
        Country = "US",
        PaymentChannel = PaymentChannel.Online,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static ScanSnapshot CreateSnapshot() => new()
    {
        Customer = new Customer { AccountNumber = "ACC001", Name = "Test" }
    };

    [Fact]
    public void AmountBelowLimit_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(5000m), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void AmountAtLimit_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(10_000m), CreateSnapshot());
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void AmountAboveLimit_Below2x_ReturnsMedium()
    {
        var result = _rule.Evaluate(CreateTransaction(15_000m), CreateSnapshot());

        Assert.True(result.IsTriggered);
        Assert.Equal("Threshold", result.RuleName);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void AmountAt2xLimit_ReturnsHigh()
    {
        var result = _rule.Evaluate(CreateTransaction(20_000m), CreateSnapshot());

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.High, result.Severity);
    }

    [Fact]
    public void AmountAbove2xBelow3x_ReturnsHigh()
    {
        var result = _rule.Evaluate(CreateTransaction(25_000m), CreateSnapshot());

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.High, result.Severity);
    }

    [Fact]
    public void AmountAt3xLimit_ReturnsCritical()
    {
        var result = _rule.Evaluate(CreateTransaction(30_000m), CreateSnapshot());

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Critical, result.Severity);
    }

    [Fact]
    public void DifferentCurrency_UsesCorrectLimit()
    {
        var result = _rule.Evaluate(CreateTransaction(200_000m, "ZAR"), CreateSnapshot());

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void UnknownCurrency_FallsBackToDefaultLimit()
    {
        var result = _rule.Evaluate(CreateTransaction(15_000m, "GBP"), CreateSnapshot());

        Assert.True(result.IsTriggered);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void UnknownCurrency_BelowDefaultLimit_ReturnsClean()
    {
        var result = _rule.Evaluate(CreateTransaction(5_000m, "GBP"), CreateSnapshot());

        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void NoDefaultLimit_UnknownCurrency_ReturnsClean()
    {
        var rule = new ThresholdRule(Options.Create(
            new ThresholdRuleOptions
            {
                Limits = new() { ["USD"] = 10_000m }
            }));

        var result = rule.Evaluate(CreateTransaction(50_000m, "GBP"), CreateSnapshot());

        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void ApplicableChannels_IncludesAll()
    {
        Assert.Contains(PaymentChannel.CardPresent, _rule.ApplicableChannels);
        Assert.Contains(PaymentChannel.Online, _rule.ApplicableChannels);
        Assert.Contains(PaymentChannel.Transfer, _rule.ApplicableChannels);
    }
}
