namespace TransactionQL.Parser

module API =
    open FParsec
    open TransactionQL.Shared.Types

    let parse filterContents =
        let filter = QLParser.parse filterContents
        match filter with
        | Success(parsedFilter, _, _) ->  Left parsedFilter
        | Failure (error, _, _) -> Right error
