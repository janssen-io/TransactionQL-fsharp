using Moq;
using System.Globalization;
using TransactionQL.DesktopApp.Models;
using TransactionQL.DesktopApp.Services;
using TransactionQL.Parser;
using static TransactionQL.Input.Converters;
using static TransactionQL.Shared.Types;

namespace TransactionQL.DesktopApp.Tests.Services;

public class DataLoaderTests
{
    private readonly SelectedData _data = new()
    {
        AccountsFile = "accounts.ldg",
        FiltersFile = "filters.tql",
        Module = "TransactionQL.Plugins.ASN.dll",
        TransactionsFile = "transactions.csv",
        HasHeader = false,
        DefaultCheckingAccount = "Assets:Checking",
        DefaultCurrency = "€",
    };

    [Fact]
    public void ReturnsError_OfParsingFilters()
    {
        // Arrange
        Mock<ITransactionQLApi> api = new();
        api.Setup(a => a.ParseFilters(It.IsAny<string>()))
            .Returns(Either<AST.Query[], string>.NewRight("My Error"));

        Mock<IStreamFiles> streamer = new();
        streamer
            .Setup(s => s.Open(_data.FiltersFile))
            .Returns(string.Empty.ToStream());

        Mock<ISelectAccounts> accounts = new();

        var loader = new DataLoader(
            api.Object, streamer.Object, accounts.Object, Directory.GetCurrentDirectory());

        // Act
        loader.TryLoadData(_data, out var trx, out string error);

        // Assert
        Assert.Empty(trx);
        Assert.Equal("My Error", error);
    }

    [Fact]
    public void ReturnsError_OfCreatingReader()
    {
        // Arrange
        Mock<ITransactionQLApi> api = new();
        api.Setup(a => a.ParseFilters(It.IsAny<string>()))
            .Returns(Either<AST.Query[], string>.NewLeft([]));

        api.Setup(a => a.LoadReader(_data.Module, It.IsAny<string>()))
            .Returns(Either<IConverter, string>.NewRight("My Error"));

        Mock<IStreamFiles> streamer = new();
        streamer
            .Setup(s => s.Open(_data.FiltersFile))
            .Returns(string.Empty.ToStream());

        streamer
            .Setup(s => s.Open(_data.FiltersFile))
            .Returns(string.Empty.ToStream());

        Mock<ISelectAccounts> accounts = new();

        var loader = new DataLoader(
            api.Object, streamer.Object, accounts.Object, Directory.GetCurrentDirectory());

        // Act
        loader.TryLoadData(_data, out var trx, out string error);

        // Assert
        Assert.Empty(trx);
        Assert.Equal("My Error", error);
    }

    [Fact]
    public void ParsesFilteredTransactions()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-us");
        var transactions = """
            13-02-2025,NL12ASNB13243546,NL98BANK75645342,AH,,,,EUR,50.00,EUR,-10.00,,,,,,,,
            14-02-2025,NL12ASNB13243546,NL98BANK75645342,AH,,,,EUR,40.00,EUR,-15.00,,,,,,,,
            """;
        var filters = """
            # Albert Heijn
            Name = "AH"

            posting {
              Assets:Checking    EUR (amount)
              Expenses:Living
            }
            """;

        Mock<ISelectAccounts> accounts = new();
        Mock<IStreamFiles> streamer = new();
        streamer
            .Setup(s => s.Open(_data.FiltersFile))
            .Returns(filters.ToStream());

        streamer
            .Setup(s => s.Open(_data.TransactionsFile))
            .Returns(transactions.ToStream());

        var api = TransactionQLApiAdapter.Instance;
        var loader = new DataLoader(
            api, streamer.Object, accounts.Object, Directory.GetCurrentDirectory());

        // Act
        loader.TryLoadData(_data, out var trx, out string error);

        // Assert
        Assert.Collection(trx,
            t =>
            {
                Assert.Equal("Albert Heijn", t.Title);
                Assert.Collection(t.Postings,
                    p =>
                    {
                        Assert.Equal("Assets:Checking", p.Account);
                        Assert.Equal("EUR", p.Currency);
                        Assert.Equal(-10m, p.Amount);
                    },
                    p =>
                    {
                        Assert.Equal("Expenses:Living", p.Account);
                        Assert.Equal("", p.Currency);
                        Assert.Null(p.Amount);
                    });
                Assert.True(t.IsValid(out string msg), msg);
            },
            t =>
            {
                Assert.Equal("Albert Heijn", t.Title);
                Assert.Collection(t.Postings,
                    p =>
                    {
                        Assert.Equal("Assets:Checking", p.Account);
                        Assert.Equal("EUR", p.Currency);
                        Assert.Equal(-15m, p.Amount);
                    },
                    p =>
                    {
                        Assert.Equal("Expenses:Living", p.Account);
                        Assert.Equal("", p.Currency);
                        Assert.Null(p.Amount);
                    });
                Assert.True(t.IsValid(out string msg), msg);
            });
    }

    [Fact]
    public void ParsesUnfilteredTransactions()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-us");
        var transactions = """
            13-02-2025,NL12ASNB13243546,NL98BANK75645342,AH,,,,EUR,50.00,EUR,-10.00,,,,,,,,
            14-02-2025,NL12ASNB13243546,NL98BANK75645342,AH,,,,EUR,40.00,EUR,-15.00,,,,,,,,
            """;
        var filters = """
            # Jumbo
            Name = "Jumbo"

            posting {
              Assets:Checking    EUR (amount)
              Expenses:Living
            }
            """;

        Mock<ISelectAccounts> accounts = new();
        Mock<IStreamFiles> streamer = new();
        streamer
            .Setup(s => s.Open(_data.FiltersFile))
            .Returns(filters.ToStream());

        streamer
            .Setup(s => s.Open(_data.TransactionsFile))
            .Returns(transactions.ToStream());

        var api = TransactionQLApiAdapter.Instance;
        var loader = new DataLoader(
            api, streamer.Object, accounts.Object, Directory.GetCurrentDirectory());

        // Act
        loader.TryLoadData(_data, out var trx, out string error);

        // Assert
        Assert.Collection(trx,
            t =>
            {
                Assert.Equal("AH", t.Title);
                Assert.Single(t.Postings);
                Assert.Equal(_data.DefaultCheckingAccount, t.Postings[0].Account);
                Assert.Equal(_data.DefaultCurrency, t.Postings[0].Currency);
                Assert.Equal(-10m, t.Postings[0].Amount);
                Assert.False(t.IsValid(out string msg), msg);
            },
            t =>
            {
                Assert.Equal("AH", t.Title);
                Assert.Single(t.Postings);
                Assert.Equal(_data.DefaultCheckingAccount, t.Postings[0].Account);
                Assert.Equal(_data.DefaultCurrency, t.Postings[0].Currency);
                Assert.Equal(-15m, t.Postings[0].Amount);
                Assert.False(t.IsValid(out string msg), msg);
            });
    }

    [Fact]
    public void ParsesMixedTransactions()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-us");
        var transactions = """
            13-02-2025,NL12ASNB13243546,NL98BANK75645342,AH,,,,EUR,50.00,EUR,-10.00,,,,,,,,
            14-02-2025,NL12ASNB13243546,NL98BANK75645342,Jumbo,,,,EUR,40.00,EUR,-15.00,,,,,,,,
            """;
        var filters = """
            # Albert Heijn
            Name = "AH"

            posting {
              Assets:Checking    EUR (amount)
              Expenses:Living
            }
            """;

        Mock<ISelectAccounts> accounts = new();
        Mock<IStreamFiles> streamer = new();
        streamer
            .Setup(s => s.Open(_data.FiltersFile))
            .Returns(filters.ToStream());

        streamer
            .Setup(s => s.Open(_data.TransactionsFile))
            .Returns(transactions.ToStream());

        var api = TransactionQLApiAdapter.Instance;
        var loader = new DataLoader(
            api, streamer.Object, accounts.Object, Directory.GetCurrentDirectory());

        // Act
        loader.TryLoadData(_data, out var trx, out string error);

        // Assert
        Assert.Collection(trx,
            t =>
            {
                Assert.Equal("Albert Heijn", t.Title);
                Assert.Collection(t.Postings,
                    p =>
                    {
                        Assert.Equal("Assets:Checking", p.Account);
                        Assert.Equal("EUR", p.Currency);
                        Assert.Equal(-10m, p.Amount);
                    },
                    p =>
                    {
                        Assert.Equal("Expenses:Living", p.Account);
                        Assert.Equal("", p.Currency);
                        Assert.Null(p.Amount);
                    });
                Assert.True(t.IsValid(out string msg), msg);
            },
            t =>
            {
                Assert.Equal("Jumbo", t.Title);
                Posting p = Assert.Single(t.Postings);
                Assert.Equal(_data.DefaultCheckingAccount, p.Account);
                Assert.Equal(_data.DefaultCurrency, p.Currency);
                Assert.Equal(-15m, p.Amount);
                Assert.False(t.IsValid(out string msg), msg);
            });
    }
}
