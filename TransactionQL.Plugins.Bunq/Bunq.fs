namespace TransactionQL.Plugins

open System
open System.IO
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
            member this.Read (FilePath fname) =
                let trxs = BunqTransactions.Load((new StreamReader(fname)))
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
                        Line (Account [fromRow "Sender"], Some ((fromRow >> Commodity) "Currency", (fromRow >> float) "Amount"))      
                        Line (Account [fromRow "Receiver"], None)
                    ]
                    Comments = 
                        [ fromRow "Description" ]
                }
