namespace TransactionQL.Plugins

open System
open System.Globalization
open FSharp.Data
open TransactionQL.Parser.AST
open TransactionQL.Parser.QLInterpreter
open TransactionQL.Input.Converters

module ING =

    let private dateFormat = "yyyy/MM/dd"

    type IngTransactions = CsvProvider<"ing.csv">

    type IngReader() =
        let toMap (row: IngTransactions.Row) =
            let isSent = row.``Af Bij`` = "Af"

            Map.ofList
                [ ("Sender", (if isSent then row.Rekening else row.Tegenrekening))
                  ("Receiver", (if isSent then row.Tegenrekening else row.Rekening))
                  ("Amount",
                   row.``Bedrag (EUR)``.Replace(",", ".")
                   |> fun amount -> if isSent then $"-%s{amount}" else amount)
                  ("Total", row.``Bedrag (EUR)``.Replace(",", "."))
                  ("Date", row.Datum.Insert(4, "/").Insert(7, "/"))
                  ("Description", row.Mededelingen)
                  ("Name", row.``Naam / Omschrijving``) ]

        interface IConverter with
            member this.DateFormat = dateFormat

            member this.Read lines =
                lines |> IngTransactions.ParseRows |> Array.map toMap

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
