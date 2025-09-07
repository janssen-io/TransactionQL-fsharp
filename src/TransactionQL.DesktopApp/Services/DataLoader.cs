using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TransactionQL.DesktopApp.Models;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.Parser;
using TransactionQL.Shared.Extensions;
using static TransactionQL.Input.Converters;
using static TransactionQL.Shared.Types;

namespace TransactionQL.DesktopApp.Services;

public interface ILoadData
{
    bool TryLoadData(SelectedData data, out IEnumerable<PaymentDetailsViewModel> payments, out string error);
}

public class DataLoader : ILoadData
{
    private static readonly Tuple<string, double> _defaultAmount = new(string.Empty, 0);
    private readonly ITransactionQLApi _api;
    private readonly IStreamFiles _streamer;
    private readonly ISelectAccounts _accountSelector;
    private readonly string _pluginDir;

    public DataLoader(
        ITransactionQLApi api, IStreamFiles streamer, ISelectAccounts accountSelector, string pluginDir)
    {
        _api = api;
        _streamer = streamer;
        _accountSelector = accountSelector;
        _pluginDir = pluginDir;
    }

    public bool TryLoadData(SelectedData data, out IEnumerable<PaymentDetailsViewModel> payments, out string error)
    {
        payments = [];

        if (!TryParseFilters(data.FiltersFile, out var queries, out error))
            return false;

        if (!TryCreateReader(data.Module, out var reader, out error, _pluginDir))
            return false;

        payments = ParseTransactions(data, queries, reader);
        return true;
    }

    private static IEnumerable<PostingViewModel> ParsePostings(QLInterpreter.Entry entry)
    {
        return entry.Lines.Select(line =>
        {
            Tuple<string, double> amountOrDefault = line.Amount.Or(_defaultAmount);

            return new PostingViewModel()
            {
                Account = string.Join(':', line.Account),
                // we don't want to display 0, if there's no amount.
                // But since Amount is a non-nullable double, we can't make it the default.
                Amount = line.Amount.HasValue() ? (decimal)amountOrDefault.Item2 : null,
                Currency = amountOrDefault.Item1,
            };
        });
    }

    private IEnumerable<PaymentDetailsViewModel> ParseTransactions(
        SelectedData data,
        AST.Query[] queries, IConverter reader)
    {
        using Stream t = _streamer.Open(data.TransactionsFile);
        using StreamReader bankTransactionCsv = new(t);
        if (data.HasHeader)
        {
            _ = bankTransactionCsv.ReadLine();
        }

        FSharpMap<string, string> variables = new([
            new("account:default", data.DefaultCheckingAccount),
        ]);

        FSharpMap<string, string>[] rows = reader.Read(bankTransactionCsv.ReadToEnd());
        Either<QLInterpreter.Entry, FSharpMap<string, string>>[] filteredRows
            = _api.Filter(reader, queries, variables, rows).ToArray();

        foreach (var filteredRow in filteredRows.Zip(rows))
        {
            yield return filteredRow.First.TryGetLeft(out QLInterpreter.Entry? entry)
                ? CreateFilteredTransaction(
                    data.DefaultCurrency, filteredRow.Second, entry)
                : CreateTransaction(
                    data.DefaultCheckingAccount, data.DefaultCurrency,
                    reader, filteredRow.Second);
        }
    }

    private PaymentDetailsViewModel CreateTransaction(
        string defaultAccount, string defaultCurrency,
        IConverter reader, FSharpMap<string, string> row)
    {
        string title = row["Name"];
        string description = row["Description"].Trim('\'');
        DateTime date = DateTime.ParseExact(row["Date"], reader.DateFormat, CultureInfo.InvariantCulture);

        decimal amount = decimal.Parse(
            row["Amount"],
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);

        var posting = new PostingViewModel { Account = defaultAccount, Currency = defaultCurrency, Amount = amount };

        return new PaymentDetailsViewModel(_accountSelector, title, date, description, defaultCurrency, amount)
        {
            Postings = { posting }
        };
    }

    private PaymentDetailsViewModel CreateFilteredTransaction(string defaultCurrency, FSharpMap<string, string> row, QLInterpreter.Entry entry)
    {
        string title = entry.Header.Item2;
        string description = string.Join(",", entry.Comments.ToArray()).Trim('\'');
        DateTime date = entry.Header.Item1;

        decimal amount = decimal.Parse(
            row["Amount"],
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);

        var postings = ParsePostings(entry);

        return new PaymentDetailsViewModel(_accountSelector, title, date, description, defaultCurrency, amount)
        {
            Postings = new(postings)
        };
    }

    private bool TryCreateReader(
        string module,
        [NotNullWhen(true)] out IConverter? reader, out string error, string pluginDir)
    {
        error = string.Empty;
        Either<IConverter, string> loader = _api.LoadReader(
            module, pluginDir);

        if (!loader.TryGetLeft(out reader))
        {
            _ = loader.TryGetRight(out error);
            return false;
        }

        return true;
    }


    private bool TryParseFilters(
        string filtersFile,
        [NotNullWhen(true)] out AST.Query[]? queries,
        out string error)
    {
        error = string.Empty;

        using Stream f = _streamer.Open(filtersFile);
        using StreamReader filterTql = new(f);

        Either<AST.Query[], string> parser = _api.ParseFilters(filterTql.ReadToEnd());

        if (!parser.TryGetLeft(out queries))
        {
            _ = parser.TryGetRight(out error);
            return false;
        }

        return true;
    }
}