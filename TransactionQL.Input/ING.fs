namespace TransactionQL.Input

open System
open System.IO
open System.Globalization
open FSharp.Data
open TransactionQL.Parser.AST
open TransactionQL.Parser.QLInterpreter


module Input =
    type IConverter =
        abstract member Read : string -> seq<Map<string, string>>
        abstract member Map : Map<string, string> -> Entry

    type Converter =
        | ING

    type IngTransactions = CsvProvider<"ing.csv">
    type ING () =
        interface IConverter with
            member this.Read fname =
                let trxs = IngTransactions.Load((new StreamReader(fname)))
                trxs.Rows
                |> Seq.map (fun row ->
                    Map.ofList [
                        ("Sender",    if row.``Af Bij`` = "Af" then row.Rekening else row.Tegenrekening)
                        ("Receiver",      if row.``Af Bij`` = "Af" then row.Tegenrekening else row.Rekening)
                        ("Amount",      row.``Bedrag (EUR)``.Replace(",", "."))
                        ("Date",        row.Datum.Insert(4, "/").Insert(7, "/"))
                        ("Description", row.Mededelingen)
                        ("Name",        row.``Naam / Omschrijving``)
                    ]
                )

            member this.Map row =
                let r col = Map.find col row
                Entry (
                    Header (
                        DateTime.ParseExact(r "Date", "yyyy/MM/dd", CultureInfo.InvariantCulture),
                        r "Name"
                    ), [
                        Line (Account [r "Receiver"], Some (Commodity "EUR", float (r "Amount")))      
                        Line (Account [r "Sender"], None)
                    ])
