namespace TransactionQL.Plugins

open System
open System.Globalization
open FSharp.Data
open TransactionQL.Parser.AST
open TransactionQL.Parser.QLInterpreter
open TransactionQL.Input.Converters

module Bunq =

    let private dateFormat = "yyyy/MM/dd"

    type BunqTransactions = CsvProvider<"bunq.csv">

    type BunqReader() =
        let toMap (row: BunqTransactions.Row) =
            let isSent = row.Amount.[0] = '-'

            Map.ofList
                [ ("Sender", (if isSent then row.Account else row.CounterParty))
                  ("Receiver", (if isSent then row.CounterParty else row.Account))
                  ("Amount", row.Amount.Replace(",", "."))
                  ("Total", (if isSent then row.Amount.Substring(1) else row.Amount))
                  ("Date", row.CreatedAt)
                  ("Description", row.Description)
                  ("Name", row.CounterPartyName)
                  ("Currency", row.Currency) ]

        interface IConverter with
            member this.DateFormat = dateFormat

            member this.Read lines =
                lines |> BunqTransactions.ParseRows |> Array.map toMap

            member this.Map row =
                let fromRow col = Map.find col row

                { Header =
                    Header(
                        DateTime.ParseExact(fromRow "Date", dateFormat, CultureInfo.InvariantCulture),
                        fromRow "Name"
                    )
                  Lines =
                    [ { Account = [ fromRow "Receiver" ]
                        Amount = (fromRow "Currency", (fromRow >> float) "Total") |> Some
                        Tag = None }
                      { Account = [ fromRow "Sender" ]
                        Amount = None
                        Tag = None } ]
                  Comments = [ fromRow "Description" ] }
