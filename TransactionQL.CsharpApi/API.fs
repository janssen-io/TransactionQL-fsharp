namespace TransactionQL.CsharpApi

open System



module API =
    open FParsec
    open TransactionQL.Input
    open TransactionQL.Parser
    open TransactionQL.Parser.AST
    open TransactionQL.Parser.QLInterpreter
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

    let formatPosting date title trx =
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

        { Header = header
          Lines = lines
          Comments = [] }
