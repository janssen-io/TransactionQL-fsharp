namespace TransactionQL.Console


module Program =
    open Argu
    open System
    open TransactionQL.Input.ING
    open TransactionQL.Input.Bunq
    open TransactionQL.Parser
    open TransactionQL.Input.Converters
    open System.IO
    open FParsec
    open Interpretation
    open Format

    type AddDescriptionFlag =
        | Always
        | Never
        | OnlyOnMissing

    type Arguments =
        | [<MainCommand; ExactlyOnce; Last>]Files of ``transactions.csv``:string * ``filter.tql``:string
        | [<AltCommandLine("-d")>]Date of format:string
        | [<AltCommandLine("-p")>]Precision of int
        | [<AltCommandLine("-c")>]Comment of chars:string
        | [<AltCommandLine("-m")>]Converter of Converter
        | [<AltCommandLine("--desc")>]AddDescription of AddDescriptionFlag
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Files (_, _) -> "the paths to the transactions and filter files"
                | Date _ -> "the date output format (default: yyyy/MM/dd)"
                | Precision _ -> "the precision of the amounts (default: 2)"
                | Comment _ -> "the character(s) to use for single line comments (default: '; ')"
                | Converter _ -> "the type of converter used to read transactions (default: ING)"
                | AddDescription _ -> "add the transaction's description below the header (default: always)"

    type Options = {
        Format : Format
        TrxFile : string
        FilterFile : string
        Reader : IConverter
        AddDescription: AddDescriptionFlag
    }

    let query options =
        let transactions = options.Reader.Read options.TrxFile
        let filter = QLParser.parse ((new StreamReader(options.FilterFile)).ReadToEnd ())
        let (sprintEntryDescription, sprintMissingDescription) =
            match options.AddDescription with
            | Always -> (List.map <| Formatter.commentLine options.Format), (List.map <| Formatter.commentLine options.Format)
            | Never -> (fun _ -> []), (fun _ -> [])
            | OnlyOnMissing -> (fun _ -> []), (List.map <| Formatter.commentLine options.Format)

        match filter with
        | Success(parsedFilter, _, _) ->
            transactions
            |> Seq.map (fun row -> { Variables = Map.empty; Row = row; DateFormat = options.Reader.DateFormat })
            |> Seq.map (fun env -> QLInterpreter.evalProgram env parsedFilter)
            |> Seq.map (
                fun (Interpretation (env, entry)) ->
                    match entry with
                    | Some entry -> 
                        Formatter.sprintPosting options.Format sprintEntryDescription id entry
                    | None -> 
                        (options.Reader.Map >> Formatter.sprintMissingPosting options.Format sprintMissingDescription) env.Row
            )
            |> Seq.map (fun lines -> String.Join(Environment.NewLine, lines))
            |> Seq.sortBy (
                fun line ->
                    if line.StartsWith options.Format.Comment then
                        line.Substring(options.Format.Comment.Length) 
                    else 
                        line
            )
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
                | ING -> mapArgs { options with Reader = new IngReader () } args'
                | Bunq -> mapArgs { options with Reader = new BunqReader () } args'
            | AddDescription wd -> mapArgs {options with AddDescription = wd } args'
           
    [<EntryPoint>]
    let main argv =
        let format = Format.ledger
        let defaultOpts = 
            { Format = format
              TrxFile = ""
              FilterFile = ""
              Reader = new IngReader ()
              AddDescription = Always }
        let argParser = ArgumentParser.Create<Arguments>(programName = "tql")

        try
            let results = argParser.Parse argv
            let args = results.GetAllResults()

            query <| mapArgs defaultOpts args
            0
        with 
        | :? ArguException as ex ->
            printfn "%s" ex.Message
            1
        | ex ->
#if DEBUG
            printfn "Error: %s" ex.Message
#endif
            2
