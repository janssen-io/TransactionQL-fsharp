namespace TransactionQL.Application

open System
open TransactionQL.Input.Converters
open TransactionQL.Parser.Interpretation



module API =
    open FParsec
    open TransactionQL.Input
    open TransactionQL.Parser
    open TransactionQL.Parser.AST
    open TransactionQL.Parser.QLInterpreter
    open TransactionQL.Parser.Interpretation
    open TransactionQL.Shared.Types

    let parseFilters filterContents =
        let filter = QLParser.parse filterContents

        match filter with
        | Success(parsedFilter, _, _) -> Left(Array.ofList parsedFilter)
        | Failure(error, _, _) -> Right error

    let loadReader name pluginDirectory =
        match PluginLoader.load name pluginDirectory with
        | Some reader -> Left reader
        | None -> Right $"Unable to load plugin from directory %s{pluginDirectory}"

    let filter (reader: IConverter) (queries: Query array) rows =
        rows
        |> Seq.map (fun row ->
            { Variables = Map.empty
              Row = row
              DateFormat = reader.DateFormat })
        |> Seq.map (fun env -> QLInterpreter.evalProgram env (List.ofArray queries))
        |> (fun results -> Seq.zip results rows)
        |> Seq.map (fun (res, row) ->
            match res with
            | Interpretation(_, Some entry) -> Left entry
            | Interpretation(_, None) -> Right row)

    let formatPosting date title (description: string) trx =
        let header = Header(date, title)

        let lines =
            trx
            |> Array.map (fun (account, (currency: string), (amount: Nullable<decimal>)) ->
                let acc = Account(account :: [])

                match (String.IsNullOrEmpty currency, Option.ofNullable amount) with
                | true, _
                | _, None ->
                    { Account = acc
                      Amount = None
                      Tag = None }
                | _, Some a ->
                    { Account = acc
                      Amount = Some(Commodity currency, float a)
                      Tag = None })
            |> List.ofArray

        let newLines = [| "\r\n"; "\n" |]

        let entry =
            { Header = header
              Lines = lines
              Comments = List.ofArray (description.Split(newLines, StringSplitOptions.RemoveEmptyEntries)) }

        let sprintDesc = (List.map <| (fun line -> $"; %s{line}"))

        Formatter.sprintPosting Format.ledger sprintDesc id entry
