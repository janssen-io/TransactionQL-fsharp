namespace TransactionQL.Application

module Format =
    type Format =
        { Date: string
          Precision: int
          Comment: string }

    let ledger: Format =
        { Date = "yyyy/MM/dd"
          Precision = 2
          Comment = "; " }

    let INDENT = "    ";

module Formatter =
    open System
    open TransactionQL.Parser.QLInterpreter
    open Format

    let sprintHeader format (Header(date, payee)) =
        $"%s{date.ToString format.Date} %s{payee}"

    let commentLine format = sprintf "%s%s" format.Comment

    let formatTags (tags: string array) =
        match tags with
        | [||] -> ""
        | xs -> Array.map (fun t -> $"{Environment.NewLine}{INDENT}; {t}") xs
                |> String.concat String.Empty

    let sprintLine
        format
        floatWidth
        ({ Account = accountParts
           Amount = amount
           Tags = tag }: Line)
        =
        let account = String.Join(":", accountParts)
        let numOfSpaces = Math.Max(0, 43 - account.Length)

        let line =
            match amount with
            | Some(commodity, sum) ->
                // Accounts and commodities must be separated by atleast two spaces
                sprintf "%s  %*s %*.*f" account numOfSpaces commodity floatWidth format.Precision sum
            | None -> account
        
        let tags = formatTags tag

        String.concat String.Empty [|INDENT; line; tags|]

    let sprintPosting format sprintDescription (modifyLine: string -> string) entry =
        let { Header = header
              Lines = lines
              Comments = comments } =
            entry

        let floatWidth =
            lines
            |> List.map (fun ({ Amount = amount }: Line) ->
                match amount with
                | Some(_, number) -> (sprintf "%.*f" format.Precision number).Length
                | None -> 0)
            |> List.max

        let headerLine = sprintHeader format header
        let commentLines = sprintDescription comments
        let transactions = List.map (sprintLine format floatWidth) lines

        headerLine :: commentLines @ transactions
        |> List.map modifyLine
        |> fun ls -> String.Join(Environment.NewLine, ls)

    let sprintMissingPosting format sprintDescription =
        sprintPosting format sprintDescription (commentLine format)
