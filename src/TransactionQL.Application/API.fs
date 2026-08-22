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

    let parseFilters filterContents =
        let filter = QLParser.parse filterContents

        match filter with
        | Success(parsedFilter, _, _) -> Result.Ok(Array.ofList parsedFilter)
        | Failure(error, _, _) -> Result.Error error

    let loadReader name pluginDirectory =
        match PluginLoader.load name pluginDirectory with
        | Some reader -> Result.Ok reader
        | None -> Result.Error $"Unable to load plugin from directory %s{pluginDirectory}"

    let filter
        (reader : IConverter)
        (queries : Query array)
        (variables : Map<string, string>)
        rows
        =
        rows
        |> Seq.map(fun row -> {
            Variables = Map.empty
            EnvVars = variables
            Row = row
            DateFormat = reader.DateFormat
        })
        |> Seq.map(fun env -> QLInterpreter.evalProgram env (List.ofArray queries))
        |> (fun results -> Seq.zip results rows)
        |> Seq.map(fun (res, row) ->
            match res with
            | Interpretation(_, Some entry) -> Choice1Of2 entry
            | Interpretation(_, None) -> Choice2Of2 row
        )

    let formatPosting date title (description : string) (tags : string list) trx =
        let header = Header(date, title)

        let lines =
            trx
            |> Array.map(fun
                             (account,
                              (currency : (string | null)),
                              (amount : Nullable<decimal>),
                              (postingTags : string array)) ->
                let acc = (account :: [])

                match (currency, Option.ofNullable amount) with
                | "", _
                | null, _
                | _, None -> {
                    Account = acc
                    Amount = None
                    Tags = postingTags
                  }
                | curr, Some a ->
                    {
                        Account = acc
                        Amount = Some(curr, float a)
                        Tags = postingTags
                    }
            )
            |> List.ofArray

        let newLines = [| "\r\n" ; "\n" |]

        let entry = {
            Header = header
            Lines = lines
            Comments =
                List.ofArray(description.Split(newLines, StringSplitOptions.RemoveEmptyEntries))
                |> List.append tags
        }

        let sprintDesc = (List.map <| (fun line -> $"{Format.INDENT}; {line}"))
        Formatter.sprintPosting Format.ledger sprintDesc id entry
