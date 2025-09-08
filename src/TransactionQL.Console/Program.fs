namespace TransactionQL.Console

module Program =
    open Argu
    open System
    open System.Globalization
    open TransactionQL.Application.Configuration
    open TransactionQL.Parser
    open TransactionQL.Input
    open TransactionQL.Input.Converters
    open TransactionQL.Input.DummyReader
    open System.IO
    open FParsec
    open Interpretation
    open TransactionQL.Application
    open TransactionQL.Application.Format

    type AddDescriptionFlag =
        | Always
        | Never
        | OnlyOnMissing

    type ParseOptions = { FilterFile: string }

    type Options =
        { Format: Format
          TrxFile: FilePath
          FilterFile: string
          Reader: IConverter
          HasHeader: bool
          Locale: string
          AddDescription: AddDescriptionFlag }

    let defaultOpts =
        { Format = Format.ledger
          TrxFile = FilePath ""
          FilterFile = ""
          Reader = new DummyReader()
          HasHeader = true
          Locale = "en_US"
          AddDescription = Always }

    type VerifyArguments =
        | [<MainCommand; ExactlyOnce>] File of string

        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | File _ -> "the path to the filter file"

    type ApplyArguments =
        | [<MainCommand; ExactlyOnce; Last>] Files of ``transactions.csv``: string * ``filter.tql``: string
        | [<AltCommandLine("-d"); Unique>] Date of format: string
        | [<AltCommandLine("-p"); Unique>] Precision of int
        | [<AltCommandLine("-c"); Unique>] Comment of chars: string
        | [<AltCommandLine("-m"); ExactlyOnce>] Converter of string
        | [<AltCommandLine("-h"); Unique>] HasHeader of bool
        | [<AltCommandLine("-l"); Unique>] Locale of string
        | [<AltCommandLine("--desc"); Unique>] AddDescription of AddDescriptionFlag
        | [<Inherit>] Version

        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Files _ -> "the paths to the transactions and filter files"
                | Date _ -> $"the date output format (default: %s{defaultOpts.Format.Date})"
                | Precision _ -> $"the precision of the amounts (default: %d{defaultOpts.Format.Precision})"
                | Comment _ ->
                    $"the character(s) to use for single line comments (default: '%s{defaultOpts.Format.Comment}')"
                | Converter _ ->
                    $"the type of converter used to read transactions. Converters are installed in: %s{createAndGetPluginDir}"
                | HasHeader _ ->
                    $"whether the transaction file contains a column header (default: %b{defaultOpts.HasHeader})"
                | Locale _ -> $"the locale used to parse decimal values (default: %s{defaultOpts.Locale})"
                | AddDescription _ ->
                    $"add the transaction's description below the header (default: %A{defaultOpts.AddDescription})"
                | Version -> $"You are running tql %s{Configuration.getAppVersion}" 

    type CliArgs =
        | [<Unique>] Version
        | [<CliPrefix(CliPrefix.None)>] Validate of ParseResults<VerifyArguments>
        | [<CliPrefix(CliPrefix.None)>] Apply of ParseResults<ApplyArguments>
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Version -> $"You are running tql %s{Configuration.getAppVersion}" 
                | Validate _ -> "Validate the filters file"
                | Apply _ -> "Apply the filters to the transactions"

    let parseFilters (options: ParseOptions) =
        let filter = QLParser.parse ((new StreamReader(options.FilterFile)).ReadToEnd())

        match filter with
        | Success(parsedFilter, _, _) -> Some parsedFilter
        | Failure(error, _, _) ->
            Console.WriteLine error
            None

    let writeLedger options parsedFilters =
        CultureInfo.CurrentCulture <- CultureInfo.CreateSpecificCulture(options.Locale)

        let (sprintEntryDescription, sprintMissingDescription) =
            let noSprint = (fun _ -> [])
            let sprintDesc = (List.map <| Formatter.commentLine options.Format)

            match options.AddDescription with
            | Always -> sprintDesc, sprintDesc
            | OnlyOnMissing -> noSprint, sprintDesc
            | Never -> noSprint, noSprint

        let sprintPosting =
            fun (Interpretation(env, entry)) ->
                match entry with
                | Some entry -> Formatter.sprintPosting options.Format sprintEntryDescription id entry
                | None ->
                    (options.Reader.Map
                     >> Formatter.sprintMissingPosting options.Format sprintMissingDescription)
                        env.Row

        let (FilePath csvFile) = options.TrxFile
        use csvStream = new StreamReader(csvFile)

        if (options.HasHeader) then
            csvStream.ReadLine() |> ignore

        csvStream.ReadToEnd()
        |> options.Reader.Read
        |> Seq.map (fun row ->
            { Variables = Map.empty
              EnvVars = Map.empty
              Row = row
              DateFormat = options.Reader.DateFormat })
        |> Seq.map (fun env -> QLInterpreter.evalProgram env parsedFilters)
        |> Seq.map sprintPosting
        |> Seq.map (fun lines -> String.Join(Environment.NewLine, lines))
        |> fun lines -> String.Join(Environment.NewLine + Environment.NewLine, lines)
        |> Console.WriteLine

    let rec mapArgs options (args: ApplyArguments list) =
        match args with
        | [] -> Some options
        | (arg :: args') ->
            match arg with
            | Files(t, f) ->
                mapArgs
                    { options with
                        TrxFile = FilePath t
                        FilterFile = f }
                    args'
            | Date d ->
                mapArgs
                    { options with
                        Format = { options.Format with Date = d } }
                    args'
            | Precision p ->
                mapArgs
                    { options with
                        Format = { options.Format with Precision = p } }
                    args'
            | Comment c ->
                mapArgs
                    { options with
                        Format = { options.Format with Comment = c } }
                    args'
            | Converter pluginName ->
                createAndGetPluginDir
                |> PluginLoader.load pluginName
                |> Option.bind (fun plugin -> mapArgs { options with Reader = plugin } args')
            | HasHeader h -> mapArgs { options with HasHeader = h } args'
            | Locale l -> mapArgs { options with Locale = l } args'
            | AddDescription wd -> mapArgs { options with AddDescription = wd } args'
            | ApplyArguments.Version -> mapArgs options args'

    [<EntryPoint>]
    let main argv =
        let argParser = ArgumentParser.Create<CliArgs>(programName = "tql")

        try
            let results = argParser.Parse argv
            let args = results.GetAllResults()

            match args with
            | [Apply applyArgs] ->
                let options = mapArgs defaultOpts (applyArgs.GetAllResults())
                let parseOptions = Option.map (fun opts -> { FilterFile = opts.FilterFile }) options

                parseOptions
                |> Option.bind parseFilters
                |> Option.map (writeLedger <| Option.get options)
                |> function
                    | Some _ -> 0
                    | None -> 2
            | [Validate validateArgs] ->
                let initOptions = { FilterFile = String.Empty }
                let options = 
                    validateArgs.GetAllResults()
                    |> List.fold 
                        (fun opts  arg -> 
                            match arg with
                            | File f -> { opts with FilterFile = f } : ParseOptions) 
                        initOptions // need some empty form of ParseOptions to get the fold started

                parseFilters options
                    |> function
                    | Some _ -> 0
                    | None -> 2
            | _ -> 
                0
        with
        | :? ArguException as ex ->
            printfn "%s" ex.Message
            1
        | ex ->
            printfn "Error: %s" ex.Message
#if DEBUG
            raise ex
#endif
            2
