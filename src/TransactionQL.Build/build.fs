open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Installer
open System

// Directory Definitions
let rootDirectory =
    __SOURCE_DIRECTORY__ </> ".." </> ".."
    |> Path.getFullName

let srcDirectory =
    rootDirectory </> "src"

let plugins = !! (srcDirectory </> "TransactionQL.Plugins.*/*proj")
let cli = srcDirectory </> "TransactionQL.Console/TransactionQL.Console.fsproj"
let gui = srcDirectory </> "TransactionQL.DesktopApp/TransactionQL.DesktopApp.csproj"

let ciDirectory =
    rootDirectory </> "ci"

let buildDirectory =
    ciDirectory </> "build"

let stagingDirectory =
    ciDirectory </> "staging"

let distDirectory =
    ciDirectory </> "dist"

let sln =
    rootDirectory </> "TransactionQL.sln"

// Build configuration helpers
let getConfiguration () =
    match Environment.environVarOrDefault "CONFIGURATION" "Release" with
    | "Debug" -> DotNet.BuildConfiguration.Debug
    | "Release" -> DotNet.BuildConfiguration.Release
    | config -> DotNet.BuildConfiguration.Custom config

let getVersion () =
    Environment.environVarOrDefault "VERSION" "0.0.0" 

/// Add a DotNet Build parameter to the list of custom parameters (-p).
let addParam key value param =
    let newParam = $"-p:{key}={value}"
    match param with
    | None -> Some newParam
    | Some prevParams -> Some $"{prevParams} {newParam}"

let withConfiguration version outputDir (c : DotNet.PublishOptions) =
    { c with 
        Common = { c.Common with CustomParams = addParam "Version" version c.Common.CustomParams }
        OutputPath = Some outputDir
    }

let appConfig (c : DotNet.PublishOptions) =
    { c with 
        Configuration = getConfiguration ()
        Common = { c.Common with CustomParams = addParam "PublishSingleFile" "true" c.Common.CustomParams }
        SelfContained = Some false
    }

/// FAKE Target Definitions
let initTargets () =
    Target.create "Clean" (fun _ ->
      Trace.log " --- Cleaning previous builds --- "
      let dirs = !! (ciDirectory) 

      dirs 
      |> Shell.cleanDirs
    )

    Target.create "Restore" (fun _ ->
      Trace.log " --- Restoring packages --- "
      DotNet.restore (fun c -> c) sln
    )

    Target.create "Test" (fun _ ->
      Trace.log " --- Testing the app --- "
      DotNet.test
          (fun c -> { c with Configuration = getConfiguration (); NoRestore = true })
          sln
    )

    Target.create "Publish" (fun _ ->
      Trace.log " --- Publishing the app --- "
      Directory.ensure buildDirectory
    )

    Target.create "Publish Plugins" (fun _ ->
      let version = getVersion ()

      plugins |> (Seq.iter (fun (s : string) ->
        DotNet.publish (fun c -> c |> withConfiguration version (buildDirectory </> "plugins")) s))
    )

    Target.create "Publish CLI" (fun _ ->
      let version = getVersion ()

      cli 
      |> DotNet.publish (fun c -> appConfig c |> withConfiguration version (buildDirectory </> "console"))
    )

    Target.create "Publish GUI" (fun _ ->
      let version = getVersion ()

      gui 
      |> DotNet.publish (fun c -> appConfig c |> withConfiguration version (buildDirectory </> "desktop"))
    )

    Target.create "Stage Artifacts" (fun _ ->
        Directory.ensure stagingDirectory
        Directory.ensure (stagingDirectory </> "plugins")
        Directory.ensure (stagingDirectory </> "desktop")

        let plugins = ["ASN"; "Bunq"; "ING"]
        let src = plugins |> List.map (fun p -> buildDirectory </> "plugins" </> $"TransactionQL.Plugins.{p}.dll")
        let dst = plugins |> List.map (fun p -> stagingDirectory </> "plugins" </> $"{String.toLower p}.dll")

        List.zip src dst
        |> List.iter (fun (s, d) -> Shell.copyFile d s)

        Shell.copyDir  (stagingDirectory </> "desktop") (buildDirectory </> "desktop") (fun f -> f.EndsWith(".dll") || f.EndsWith(".exe"))
        Shell.copyFile (stagingDirectory </> "tql.exe") (buildDirectory </> "console" </> "TransactionQL.Console.exe")
    )

    Target.create "Dist" (fun _ ->
      Directory.ensure distDirectory
    )

    Target.create "Setup" (fun _ ->
      let setup = __SOURCE_DIRECTORY__ </> "tql.iss"
      let backup = $"{setup}.bk"

      // Keep original for when building locally
      Shell.copyFile backup setup
      Shell.replaceInFiles 
        [
          ("{version}", getVersion ())
          ("{sourceDir}", stagingDirectory)
        ] [__SOURCE_DIRECTORY__ </> "tql.iss"]

      try
        InnoSetup.build(fun p ->
          { p with
              OutputFolder = distDirectory
              ScriptFile = __SOURCE_DIRECTORY__ </> "tql.iss"
          }
        )
      finally
        Shell.mv backup setup
    )

    Target.create "Archive" (fun _ ->
      Trace.log " --- Creating app archive --- "
      let os = if OperatingSystem.IsWindows () then "windows" else "linux"
      let filename = distDirectory </> $"tql-{os}-x64.zip"

      (!! stagingDirectory)
      |> Zip.zip stagingDirectory filename 
    )

    Target.create "Vim" (fun _ ->
      Trace.log " --- Copy Vim Highlighting --- "
      Shell.copyFile distDirectory (__SOURCE_DIRECTORY__ </> "tql.vim")
    )

    // Nothing to do, just to have a single node at the end of the dependency graph
    Target.create "Complete" (fun _ -> ( Trace.log "✅ Job's done!" ))

    "Clean" <=> "Restore"
      =?> ("Test", Environment.hasEnvironVar "SkipTests" |> not)
      ==> "Publish"
      ==> "Publish Plugins" <=> "Publish CLI" <=> "Publish GUI"
      ==> "Stage Artifacts" <=> "Dist"
      =?> ("Setup", OperatingSystem.IsWindows ()) <=> "Archive" <=> "Vim"
      ==> "Complete"

[<EntryPoint>]
let main args =
    args
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    Target.initEnvironment()

    initTargets ()
    |> ignore

    Target.runOrDefault "Complete"
    0
