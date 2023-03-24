namespace TransactionQL.Console

module Format =
    type Format =
        { Date: string
          Precision: int
          Comment: string }

    let ledger: Format =
        { Date = "yyyy/MM/dd"
          Precision = 2
          Comment = "; " }

module Formatter =
    open System
    open TransactionQL.Parser.QLInterpreter
    open TransactionQL.Parser.AST
    open Format

    let sprintHeader format (Header(date, payee)) =
        sprintf "%s %s" (date.ToString format.Date) payee

    let commentLine format = sprintf "%s%s" format.Comment

    let sprintLine
        format
        floatWidth
        ({ Account = Account accountParts
           Amount = amount
           Tag = tag }: Line)
        =
        let account = String.Join(":", accountParts)
        let numOfSpaces = Math.Max(0, 43 - account.Length)

        let line =
            match amount with
            | Some(Commodity commodity, sum) ->
                // Accounts and commodities must be separated by atleast two spaces
                sprintf "%s  %*s %*.*f" account numOfSpaces commodity floatWidth format.Precision sum
            | None -> account

        match tag with
        | Some text -> sprintf "%s  ; %s" line text
        | None -> line
        |> sprintf "    %s" // indent lines

    let sprintPosting format sprintDescription (modifyLine: string -> string) entry =
        let { Header = header
              Lines = lines
              Comments = comments } =
            entry

        let floatWidth =
            lines
            |> List.map (fun ({ Amount = amount }: Line) ->
                match amount with
                | Some(Commodity _, number) -> (sprintf "%.*f" format.Precision number).Length
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
