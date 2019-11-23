namespace TransactionQL.Console

module Program =
    open Argu
    open System
    open TransactionQL.Input.Input
    open TransactionQL.Parser
    open System.IO
    open FParsec
    open Interpretation
    open Format

    type Arguments =
        | [<MainCommand; ExactlyOnce; Last>]Files of ``transactions.csv``:string * ``filter.tql``:string
        | Date of format:string
        | Precision of int
        | Comment of chars:string
        | Converter of TransactionQL.Input.Input.Converter
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Files (_, _) -> "the path to the transactions and filter file"
                | Date _ -> "the date output format"
                | Precision _ -> "the precision of the amounts"
                | Comment _ -> "the character(s) to use for single line comments"
                | Converter _ -> "the type of converter used to read transactions"

    type Options = {
        Format : Format
        TrxFile : string
        FilterFile : string
        Reader : IConverter
    }

    let query options =
        let transactions = options.Reader.Read options.TrxFile
        let filter = QLParser.parse ((new StreamReader(options.FilterFile)).ReadToEnd ())
        match filter with
        | Success(parsedFilter, _, _) ->
            transactions
            |> Seq.map (fun row -> { Variables = Map.empty; Row = row })
            |> Seq.map (fun env -> QLInterpreter.evalProgram env parsedFilter)
            |> Seq.map (fun (Interpretation (env, entry)) ->
                match entry with
                | Some entry -> Formatter.sprintPosting options.Format entry id
                | None -> (options.Reader.Map >> Formatter.sprintMissingPosting options.Format) env.Row)
            |> Seq.map (fun lines -> String.Join(Environment.NewLine, lines))
            |> fun lines -> String.Join(Environment.NewLine + Environment.NewLine, lines)
            |> Console.WriteLine
        | Failure (error, _, _) -> Console.WriteLine error

    let rec mapArgs options (args : Arguments list) = 
        match args with
        | [] -> options
        | (arg::args') ->
            match arg with
            | Files (t,f) -> mapArgs { options with TrxFile = t; FilterFile = f } args'
            | Date d -> mapArgs { options with Format = { options.Format with Date = d }} args'
            | Precision p -> mapArgs { options with Format = { options.Format with Precision = p }} args'
            | Comment c -> mapArgs { options with Format = { options.Format with Comment = c }} args'
            | Converter c -> 
                match c with
                | ING -> mapArgs { options with Reader = new ING () } args'
           
    [<EntryPoint>]
    let main argv =
        let format = Format.ledger
        let defaultOpts = { Format = format; TrxFile = ""; FilterFile = ""; Reader = new ING () }
        let argParser = ArgumentParser.Create<Arguments>(programName = "tql")

        let results = argParser.Parse argv
        let args = results.GetAllResults()

        query <| mapArgs defaultOpts args
        0
