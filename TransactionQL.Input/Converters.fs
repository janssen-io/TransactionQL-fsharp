namespace TransactionQL.Input

open TransactionQL.Parser.QLInterpreter
open TransactionQL.Parser.Interpretation

module Converters =

    type IConverter =
        abstract member Read : string -> seq<Row>
        abstract member Map : Row -> Entry
        abstract member DateFormat : string

    type Converter =
        | ING
        | Bunq


