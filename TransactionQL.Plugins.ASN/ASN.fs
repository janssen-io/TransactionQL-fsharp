namespace TransactionQL.Plugins

open System
open System.IO
open System.Globalization
open FSharp.Data
open TransactionQL.Parser.AST
open TransactionQL.Parser.QLInterpreter
open TransactionQL.Input.Converters

module ING =

    let private dateFormat = "dd-MM-yyyy"

    type AsnTransactions = CsvProvider<"ASN.csv">
    type IngReader () =
        interface IConverter with
            member this.DateFormat = dateFormat;
            member this.Read (FilePath fname) =
                let trxs = AsnTransactions.Load((new StreamReader(fname)))
                trxs.Rows
                |> Seq.map (fun row ->
                    let amount = Decimal.Parse(row.Transactiebedrag, NumberStyles.AllowLeadingSign ||| NumberStyles.AllowDecimalPoint)
                    let isSent = amount < 0m
                    Map.ofList [
                        ("Sender",      if isSent then row.Opdrachtgeversrekening else row.Tegenrekeningsnummer)
                        ("Receiver",    if isSent then row.Tegenrekeningsnummer else row.Opdrachtgeversrekening)
                        ("Amount",      string amount)
                        ("Total",       (string <| Math.Abs amount))
                        ("Date",        row.Boekingsdatum)
                        ("Description", row.Omschrijving)
                        ("Name",        row.``Naam Tegenrekening``)
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
                        Line (Account [fromRow "Sender"], Some (Commodity "EUR", float (fromRow "Amount")))      
                        Line (Account [fromRow "Receiver"], None)
                    ]
                    Comments = 
                        [ fromRow "Description" ]
                }
