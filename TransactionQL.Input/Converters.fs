namespace TransactionQL.Input

open TransactionQL.Parser.QLInterpreter
open TransactionQL.Parser.Interpretation
open System.IO

module Converters =

    type FilePath = FilePath of string

    type IConverter =
        abstract member Read : StreamReader -> seq<Row>
        abstract member Map : Row -> Entry
        abstract member DateFormat : string

