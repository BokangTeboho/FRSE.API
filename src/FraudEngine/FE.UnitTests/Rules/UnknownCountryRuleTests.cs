using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Infrastructure.Rules;

namespace FE.UnitTests.Rules;

public class UnknownCountryRuleTests
{
    private readonly UnknownCountryRule _rule = new();

    private static Transaction CreateTransaction(string country = "NG") => new()
    {
        AccountNumber = "ACC001",
        ReferenceId = "REF001",
        Amount = 100m,
        Currency = "USD",
        Country = country,
        PaymentChannel = PaymentChannel.CardPresent,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public void NoKnownCountries_ReturnsClean()
    {
        var snapshot = new ScanSnapshot
        {
            Customer = new Customer
            {
                AccountNumber = "ACC001",
                Name = "Test",
                KnownCountries = []
            }
        };

        var result = _rule.Evaluate(CreateTransaction(), snapshot);
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void CountryInKnownList_ReturnsClean()
    {
        var snapshot = new ScanSnapshot
        {
            Customer = new Customer
            {
                AccountNumber = "ACC001",
                Name = "Test",
                KnownCountries = ["US", "GB", "NG"]
            }
        };

        var result = _rule.Evaluate(CreateTransaction("NG"), snapshot);
        Assert.False(result.IsTriggered);
    }

    [Fact]
    public void CountryNotInKnownList_ReturnsMedium()
    {
        var snapshot = new ScanSnapshot
        {
            Customer = new Customer
            {
                AccountNumber = "ACC001",
                Name = "Test",
                KnownCountries = ["US", "GB"]
            }
        };

        var result = _rule.Evaluate(CreateTransaction("NG"), snapshot);

        Assert.True(result.IsTriggered);
        Assert.Equal("UnknownCountry", result.RuleName);
        Assert.Equal(Severity.Medium, result.Severity);
    }

    [Fact]
    public void OnlyAppliesToCardPresent()
    {
        Assert.Single(_rule.ApplicableChannels);
        Assert.Contains(PaymentChannel.CardPresent, _rule.ApplicableChannels);
    }
}
