using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TransactionQL.Application;
using TransactionQL.Parser;
using static TransactionQL.Input.Converters;
using static TransactionQL.Parser.AST;
using static TransactionQL.Shared.Types;

namespace TransactionQL.DesktopApp.Services;

using Row = FSharpMap<string, string>;

public interface ITransactionQLApi
{
    IEnumerable<Either<QLInterpreter.Entry, Row>> Filter(IConverter reader, Query[] queries, IEnumerable<Row> rows);
    string FormatPosting(DateTime date, string title, string description, Tuple<string, string, decimal?>[] trx);
    Either<IConverter, string> LoadReader(string name, string pluginDirectory);
    Either<Query[], string> ParseFilters(string filterContents);
}

public class TransactionQLApiAdapter : ITransactionQLApi
{
    public static readonly TransactionQLApiAdapter Instance = new();

    public IEnumerable<Either<QLInterpreter.Entry, Row>> Filter(IConverter reader, Query[] queries, IEnumerable<Row> rows)
        => API.filter(reader, queries, rows);

    public string FormatPosting(DateTime date, string title, string description, Tuple<string, string, decimal?>[] trx)
        => API.formatPosting(date, title, description, trx);

    public Either<IConverter, string> LoadReader(string name, string pluginDirectory)
        => API.loadReader(name, pluginDirectory);

    public Either<Query[], string> ParseFilters(string filterContents)
        => API.parseFilters(filterContents);
}