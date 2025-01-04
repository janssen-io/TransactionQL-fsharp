namespace TransactionQL.Plugins

open System
open System.Globalization
open FSharp.Data
open TransactionQL.Parser.AST
open TransactionQL.Parser.QLInterpreter
open TransactionQL.Input.Converters
open TransactionQL.Shared.Disposables

module Amex =

    let private dateFormat = "MM/dd/yyyy"

    type AmexTransactions = CsvProvider<"Amex.csv">

    type AmexReader() =
        let toMap (row: AmexTransactions.Row) =
            let amount =
                -1m * Decimal.Parse(row.Bedrag, NumberStyles.AllowLeadingSign ||| NumberStyles.AllowDecimalPoint)

            let isSent = amount < 0m

            Map.ofList
                [ ("Sender", if isSent then row.Omschrijving else row.Kaartlid)
                  ("Receiver", if isSent then row.Kaartlid else row.Omschrijving)
                  ("Amount", string amount)
                  ("Total", (string <| Math.Abs amount))
                  ("Date", row.Datum)
                  ("Description", row.Omschrijving)
                  ("Name", row.Omschrijving) ]

        interface IConverter with
            member this.DateFormat = dateFormat

            member this.Read lines =
                using (Disposables.changeCulture "nl-NL") (fun _ -> 
                    lines |> AmexTransactions.ParseRows |> Array.map toMap
                )

            member this.Map row =
                let fromRow col = Map.find col row

                { Header =
                    Header(
                        DateTime.ParseExact(fromRow "Date", dateFormat, CultureInfo.InvariantCulture),
                        fromRow "Name"
                    )
                  Lines =
                    [ { Account = Account [ fromRow "Receiver" ]
                        Amount = (Commodity "EUR", float (fromRow "Total")) |> Some
                        Tag = None }
                      { Account = Account [ fromRow "Sender" ]
                        Amount = None
                        Tag = None } ]
                  Comments = [ fromRow "Description" ] }
