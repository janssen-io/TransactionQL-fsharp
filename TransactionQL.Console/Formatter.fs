namespace TransactionQL.Console

module Format =
    type Format = {
        Date: string
        Precision: int
        Comment: string
    }

    let ledger : Format =  {
        Date = "yyyy/MM/dd"
        Precision = 2
        Comment = "; "
    }

module Formatter = 
    open System
    open TransactionQL.Parser.QLInterpreter
    open TransactionQL.Parser.AST
    open Format

    let sprintHeader format (Header (date, payee)) =
        sprintf "%s %s" (date.ToString format.Date) payee

    let sprintLine format floatWidth (Line (Account accountParts, amount)) =
        let account = String.Join (":", accountParts)
        let numOfSpaces = Math.Max(0, 43 - account.Length)
        match amount with
        | Some (Commodity commodity, sum) ->
            // Accounts and commodities should be separated by atleast two spaces
            sprintf "%s  %*s %*.*f" account numOfSpaces commodity floatWidth format.Precision sum
        | None -> account
        |> sprintf "    %s" // indent lines

    let sprintPosting format (Entry (header, lines)) (lineModifier : string list -> string list) =
        let floatWidth = 
            lines
            |> List.map (fun (Line (Account _, a)) -> 
                match a with
                | Some (Commodity _, f) -> (sprintf "%.*f" format.Precision f).Length
                | None -> 0)
            |> List.max

        (sprintHeader format header) :: (List.map (sprintLine format floatWidth) lines)
        |> lineModifier
        |> fun ls -> String.Join(Environment.NewLine, ls)

    let sprintMissingPosting format entry = 
        sprintPosting format entry (List.map (fun line -> format.Comment + line))

