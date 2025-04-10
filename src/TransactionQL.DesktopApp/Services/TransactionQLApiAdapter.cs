﻿using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using TransactionQL.Application;
using TransactionQL.Parser;
using static TransactionQL.Input.Converters;
using static TransactionQL.Parser.AST;
using static TransactionQL.Shared.Types;

namespace TransactionQL.DesktopApp.Services;

using Row = FSharpMap<string, string>;

/// <summary>
/// Interface to make components that interact with <see cref="TransactionQL.Application" /> testable.
/// </summary>
public interface ITransactionQLApi
{
    IEnumerable<Either<QLInterpreter.Entry, Row>> Filter(
        IConverter reader,
        Query[] queries,
        FSharpMap<string, string> variables,
        IEnumerable<Row> rows);

    string FormatPosting(DateTime date, string title, string description, string[] tags, Tuple<string, string?, decimal?, string[]>[] trx);

    Either<IConverter, string> LoadReader(string name, string pluginDirectory);

    Either<Query[], string> ParseFilters(string filterContents);
}

public class TransactionQLApiAdapter : ITransactionQLApi
{
    public static readonly TransactionQLApiAdapter Instance = new();

    public IEnumerable<Either<QLInterpreter.Entry, Row>> Filter(
        IConverter reader,
        Query[] queries,
        FSharpMap<string, string> variables,
        IEnumerable<Row> rows)
        => API.filter(reader, queries, variables, rows);

    public string FormatPosting(DateTime date, string title, string description, string[] tags, Tuple<string, string?, decimal?, string[]>[] trx)
        => API.formatPosting(date, title, description, FSharpList.Create<string>(tags), trx);

    public Either<IConverter, string> LoadReader(string name, string pluginDirectory)
        => API.loadReader(name, pluginDirectory);

    public Either<Query[], string> ParseFilters(string filterContents)
        => API.parseFilters(filterContents);
}