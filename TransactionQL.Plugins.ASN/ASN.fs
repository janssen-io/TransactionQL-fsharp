namespace TransactionQL.Plugins

open System
open System.Globalization
open FSharp.Data
open TransactionQL.Parser.AST
open TransactionQL.Parser.QLInterpreter
open TransactionQL.Input.Converters

module ASN =

    let private dateFormat = "dd-MM-yyyy"

    type AsnTransactions = CsvProvider<"ASN.csv">

    type AsnReader () =
        interface IConverter with
            member this.DateFormat = dateFormat;
            member this.Read csvStream =
                let rows = csvStream.ReadToEnd()
                let trxs = AsnTransactions.ParseRows(rows)
                trxs
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
                        Line (Account [fromRow "Receiver"], Some (Commodity "EUR", float (fromRow "Total")))
                        Line (Account [fromRow "Sender"], None)
                    ]
                    Comments = 
                        [ fromRow "Description" ]
                }
