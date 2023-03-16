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
    type BunqReader () =
        interface IConverter with
            member this.DateFormat = dateFormat
            member this.Read csvStream =
                let trxs = BunqTransactions.Load(csvStream)
                trxs.Rows
                |> Seq.map (fun row ->
                    let isSent = row.Amount.[0] = '-'
                    Map.ofList [
                        ("Sender",      if isSent then row.Account else row.CounterParty)
                        ("Receiver",    if isSent then row.CounterParty else row.Account)
                        ("Amount",      row.Amount.Replace(",", "."))
                        ("Total",       if isSent
                                        then row.Amount.Substring(1)
                                        else row.Amount)
                        ("Date",        row.CreatedAt)
                        ("Description", row.Description)
                        ("Name",        row.CounterPartyName)
                        ("Currency",    row.Currency)
                    ]
                )

            member this.Map row = 
                let fromRow col = Map.find col row
                {
                    Header = 
                        Header (
                            DateTime.ParseExact(fromRow "Date", dateFormat, CultureInfo.InvariantCulture),
                            fromRow "Name"
                        )
                    Lines = [
                        { 
                            Account = Account [fromRow "Receiver"]
                            Amount = ((fromRow >> Commodity) "Currency", (fromRow >> float) "Total") |> Some
                            Tag = None
                        }
                        { 
                            Account = Account [fromRow "Sender"]
                            Amount = None
                            Tag = None
                        }
                    ]
                    Comments = [ fromRow "Description" ]
                }
