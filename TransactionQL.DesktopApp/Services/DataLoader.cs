using Avalonia.Threading;
using DynamicData;
using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using TransactionQL.Application;
using TransactionQL.DesktopApp.Models;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.Parser;
using TransactionQL.Shared.Extensions;
using static TransactionQL.Input.Converters;
using static TransactionQL.Shared.Types;

namespace TransactionQL.DesktopApp.Services;

public class DataLoader : ILoadData
{

    private static readonly Tuple<AST.Commodity, double> _defaultAmount = new(AST.Commodity.NewCommodity(""), 0);

    private readonly SelectedData _data;
    private readonly FilewatchingAccountSelector _accountSelector;

    public DataLoader(SelectedData data)
    {
        _data = data;
        _accountSelector = FilewatchingAccountSelector.Monitor(data.AccountsFile, Dispatcher.UIThread.Invoke);
    }

    public bool TryLoadData(out IEnumerable<PaymentDetailsViewModel> payments)
    {
        payments = [];

        if (!TryParseFilters(_data.FiltersFile, out var queries))
            return false;

        if (!TryCreateReader(_data, out var reader))
            return false;

        payments = ParseTransactions(_data, queries, reader);
        return true;
    }

    private static IEnumerable<Posting> ParsePostings(QLInterpreter.Entry entry)
    {
        return entry.Lines.Select(line =>
        {
            Tuple<AST.Commodity, double> amountOrDefault = line.Amount.Or(_defaultAmount);

            return new Posting()
            {
                Account = string.Join(':', line.Account.Item),
                // we don't want to display 0, if there's no amount.
                // But since Amount is a non-nullable double, we can't make it the default.
                Amount = line.Amount.HasValue() ? (decimal)amountOrDefault.Item2 : null,
                Currency = amountOrDefault.Item1.Item,
            };
        });
    }

    private IEnumerable<PaymentDetailsViewModel> ParseTransactions(SelectedData data, AST.Query[] queries, IConverter reader)
    {
        // TODO: external dependency (IO)
        using StreamReader bankTransactionCsv = new(data.TransactionsFile);
        if (data.HasHeader)
        {
            _ = bankTransactionCsv.ReadLine();
        }

        FSharpMap<string, string>[] rows = reader.Read(bankTransactionCsv.ReadToEnd());
        Either<QLInterpreter.Entry, FSharpMap<string, string>>[] filteredRows = API.filter(reader, queries, rows).ToArray();

        foreach(var filteredRow in filteredRows.Zip(rows))
        {
            yield return filteredRow.First.TryGetLeft(out QLInterpreter.Entry? entry)
                ? CreateFilteredTransaction(filteredRow.Second, entry)
                : CreateTransaction(reader, filteredRow.Second);
        }
    }

    private PaymentDetailsViewModel CreateTransaction(IConverter reader, FSharpMap<string, string> row)
    {
        string title = row["Name"];
        string description = row["Description"].Trim('\'');
        DateTime date = DateTime.ParseExact(row["Date"], reader.DateFormat, CultureInfo.InvariantCulture);
        decimal amount = decimal.Parse(row["Amount"],
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);
        var posting = new Posting { Account = _data.DefaultCheckingAccount, Currency = _data.DefaultCurrency, Amount = amount };

        return new PaymentDetailsViewModel(_accountSelector, title, date, description, _data.DefaultCurrency, amount)
        {
            Postings = { posting }
        };
    }

    private PaymentDetailsViewModel CreateFilteredTransaction(FSharpMap<string, string> row, QLInterpreter.Entry entry)
    {
        string title = entry.Header.Item2;
        string description = string.Join(",", entry.Comments.ToArray()).Trim('\'');
        DateTime date = entry.Header.Item1;
        decimal amount = decimal.Parse(row["Amount"],
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);
        var postings = ParsePostings(entry);

        return new PaymentDetailsViewModel(_accountSelector, title, date, description, _data.DefaultCurrency, amount)
        {
            Postings = new(postings)
        };
    }

    private bool TryCreateReader(
        SelectedData data, [NotNullWhen(true)] out IConverter? reader)
    {
        Either<IConverter, string> loader = API.loadReader(data.Module, Configuration.createAndGetPluginDir);
        if (!loader.TryGetLeft(out reader))
        {
            _ = loader.TryGetRight(out string? message);
            // TODO: external dependency (ViewModel)
            //ErrorThrown?.Invoke(this, new ErrorViewModel(message));
            return false;
        }

        return true;
    }


    private bool TryParseFilters(string filtersFile, [NotNullWhen(true)] out AST.Query[]? queries)
    {
        // TODO: external dependency (IO)
        using StreamReader filterTql = new(filtersFile);
        Either<AST.Query[], string> parser = API.parseFilters(filterTql.ReadToEnd());
        if (!parser.TryGetLeft(out queries))
        {
            _ = parser.TryGetRight(out string? message);
            // TODO: external dependency (ViewModel)
            //ErrorThrown?.Invoke(this, new ErrorViewModel(message));
            return false;
        }

        return true;
    }
}

public interface ILoadData
{
    bool TryLoadData(out IEnumerable<PaymentDetailsViewModel> payments);
}