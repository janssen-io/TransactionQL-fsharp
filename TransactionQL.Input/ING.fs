namespace TransactionQL.Input

open System
open System.IO
open System.Globalization
open FSharp.Data
open TransactionQL.Parser.AST
open TransactionQL.Parser.QLInterpreter
open TransactionQL.Input.Converters

module ING =

    type IngTransactions = CsvProvider<"ing.csv">
    type IngReader () =
        interface IConverter with
            member this.DateFormat = "yyyy/MM/dd"
            member this.Read fname =
                let trxs = IngTransactions.Load((new StreamReader(fname)))
                trxs.Rows
                |> Seq.map (fun row ->
                    Map.ofList [
                        ("Sender",      if row.``Af Bij`` = "Af" then row.Rekening else row.Tegenrekening)
                        ("Receiver",    if row.``Af Bij`` = "Af" then row.Tegenrekening else row.Rekening)
                        ("Amount",      row.``Bedrag (EUR)``.Replace(",", ".")
                                        |> fun amount ->
                                           if row.``Af Bij`` = "Af"
                                           then sprintf "-%s" amount
                                           else amount)
                        ("Total",       row.``Bedrag (EUR)``.Replace(",", "."))
                        ("Date",        row.Datum.Insert(4, "/").Insert(7, "/"))
                        ("Description", row.Mededelingen)
                        ("Name",        row.``Naam / Omschrijving``)
                    ]
                )

            member this.Map row =
                let fromRow col = Map.find col row
                {
                    Header = 
                        Header (
                            DateTime.ParseExact(fromRow "Date", "yyyy/MM/dd", CultureInfo.InvariantCulture),
                            fromRow "Name"
                        )
                    Lines = [
                        Line (Account [fromRow "Sender"], Some (Commodity "EUR", float (fromRow "Amount")))      
                        Line (Account [fromRow "Receiver"], None)
                    ]
                    Comments = 
                        [ fromRow "Description" ]
                }
