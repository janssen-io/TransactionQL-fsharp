namespace TransactionQL.Input

open TransactionQL.Parser.QLInterpreter
open TransactionQL.Parser.Interpretation

module Converters =

    type FilePath = FilePath of string

    type IConverter =
        abstract member Read : FilePath -> seq<Row>
        abstract member Map : Row -> Entry
        abstract member DateFormat : string

