namespace TransactionQL.Input

open TransactionQL.Parser.QLInterpreter
open TransactionQL.Parser.Interpretation

module Converters =

    type FilePath = FilePath of string

    type IConverter =
        // Read takes the lines and parses them. This interface does not know whether we deal with single or multi-line transactions.
        abstract member Read : string -> Row array
        abstract member Map : Row -> Entry
        abstract member DateFormat : string

