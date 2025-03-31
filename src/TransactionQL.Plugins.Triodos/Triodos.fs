namespace TransactionQL.Plugins

open System
open System.Globalization
open FSharp.Data
open TransactionQL.Parser.AST
open TransactionQL.Parser.QLInterpreter
open TransactionQL.Input.Converters
open TransactionQL.Shared.Disposables

module Triodos =

    let private dateFormat = "dd-MM-yyyy"

    type TriodosTransactions = CsvProvider<"triodos.csv">

    type TriodosReader() =
        let toMap (row: TriodosTransactions.Row) =
            let isSent = row.``Credit/Debet`` = "Debet"

            let preppedAmount = row.Bedrag.Replace(".", "").Replace(",", ".")

            Map.ofList
                [ ("Sender", (if isSent then row.Rekening else row.Tegenrekening))
                  ("Receiver", (if isSent then row.Tegenrekening else row.Rekening))
                  ("Amount", preppedAmount
                   |> fun amount -> if isSent then $"-%s{amount}" else amount)
                  ("Total", preppedAmount)
                  ("Date", row.Transactiedatum)
                  ("Description", $"[{row.Transactiecode}] {row.Omschrijving}")
                  ("Name", row.``Naam Tegenrekening``) ]

        interface IConverter with
            member this.DateFormat = dateFormat

            member this.Read lines =
                using (Disposables.changeCulture "nl-NL") (fun _ -> 
                    lines |> TriodosTransactions.ParseRows |> Array.map toMap
                )

            member this.Map row =
                let fromRow col = Map.find col row

                { Header =
                    Header(
                        DateTime.ParseExact(fromRow "Date", dateFormat, CultureInfo.InvariantCulture),
                        fromRow "Name"
                    )
                  Lines =
                    [ { Account = [ fromRow "Receiver" ]
                        Amount = ("EUR", float (fromRow "Total")) |> Some
                        Tags = [||] }
                      { Account = [ fromRow "Sender" ]
                        Amount = None
                        Tags = [||] } ]
                  Comments = [ fromRow "Description" ] }
