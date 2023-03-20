namespace TransactionQL.CsharpApi

open TransactionQL.Input.Converters


module API =
    open FParsec
    open TransactionQL.Input
    open TransactionQL.Parser
    open TransactionQL.Shared.Types

    let parseFilters filterContents =
        let filter = QLParser.parse filterContents
        match filter with
        | Success(parsedFilter, _, _) ->  Left (Array.ofList parsedFilter)
        | Failure (error, _, _) -> Right error

    let loadReader name pluginDirectory =
        match PluginLoader.load name pluginDirectory with
        | Some reader -> Left reader
        | None -> Right (sprintf "Unable to load plugin from directory %s" pluginDirectory)

