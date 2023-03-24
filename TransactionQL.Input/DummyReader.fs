namespace TransactionQL.Input

open Converters

module DummyReader =

    type DummyReader() =
        interface IConverter with
            member this.DateFormat = "yyyy/MM/dd"
            member this.Read _ = failwith "Not implemented"
            member this.Map _ = failwith "Not implemented"
