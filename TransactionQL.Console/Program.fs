namespace TransactionQL.Console

open TransactionQL.Input

module Program =
    open Argu
    open System
    open TransactionQL.Parser
    open TransactionQL.Input.Converters
    open TransactionQL.Input.DummyReader
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
        | [<AltCommandLine("-m"); ExactlyOnce>]Converter of string
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
        TrxFile : FilePath
        FilterFile : string
        Reader : IConverter
        AddDescription: AddDescriptionFlag
    }

    let createAndGetAppDir =
        let appDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        let dir = Path.Combine(appDir, "tql")
        Directory.CreateDirectory dir |> ignore
        dir

    let createAndGetPluginDir =
        let appDir = createAndGetAppDir
        let dir = Path.Combine(appDir, "plugins")
        Directory.CreateDirectory dir |> ignore
        dir

    let parseFilters options =
        let filter = QLParser.parse ((new StreamReader(options.FilterFile)).ReadToEnd ())
        match filter with
        | Success(parsedFilter, _, _) -> Some parsedFilter
        | Failure (error, _, _) ->
            Console.WriteLine error
            None

    let writeLedger options parsedFilter =
        let (sprintEntryDescription, sprintMissingDescription) =
            let noSprint = (fun _ -> [])
            let sprintDesc = (List.map <| Formatter.commentLine options.Format)
            match options.AddDescription with
            | Always -> sprintDesc, sprintDesc 
            | OnlyOnMissing -> noSprint, sprintDesc
            | Never -> noSprint, noSprint

        let sprintPosting =
            fun (Interpretation (env, entry)) ->
                match entry with
                | Some entry -> 
                    Formatter.sprintPosting options.Format sprintEntryDescription id entry
                | None -> 
                    (options.Reader.Map >> Formatter.sprintMissingPosting options.Format sprintMissingDescription) env.Row

        options.Reader.Read options.TrxFile
        |> Seq.map (fun row -> { Variables = Map.empty; Row = row; DateFormat = options.Reader.DateFormat })
        |> Seq.map (fun env -> QLInterpreter.evalProgram env parsedFilter)
        |> Seq.map sprintPosting
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

    let rec mapArgs options (args : Arguments list) = 
        match args with
        | [] -> Some options
        | (arg::args') ->
            match arg with
            | Files (t,f) ->
                mapArgs { options with TrxFile = FilePath t; FilterFile = f } args'
            | Date d ->
                mapArgs { options with Format = { options.Format with Date = d }} args'
            | Precision p -> 
                mapArgs { options with Format = { options.Format with Precision = p }} args'
            | Comment c -> 
                mapArgs { options with Format = { options.Format with Comment = c }} args'
            | Converter pluginName -> 
                createAndGetPluginDir
                |> PluginLoader.load pluginName
                |> Option.bind (fun plugin -> mapArgs { options with Reader = plugin } args')
            | AddDescription wd -> 
                mapArgs {options with AddDescription = wd } args'
           
    [<EntryPoint>]
    let main argv =
        let format = Format.ledger
        let defaultOpts = 
            { Format = format
              TrxFile = FilePath ""
              FilterFile = ""
              Reader = new DummyReader ()
              AddDescription = Always }
        let argParser = ArgumentParser.Create<Arguments>(programName = "tql")

        try
            let results = argParser.Parse argv
            let args = results.GetAllResults()

            let options = mapArgs defaultOpts args

            options
            |> Option.bind parseFilters 
            |> Option.map (writeLedger <| Option.get options)
            |> function
            | Some _ -> 0
            | None -> 2
        with 
        | :? ArguException as ex ->
            printfn "%s" ex.Message
            1
        | ex ->
#if DEBUG
            printfn "Error: %s" ex.Message
#endif
            2
