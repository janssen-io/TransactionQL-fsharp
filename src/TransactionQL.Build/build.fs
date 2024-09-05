open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

let rootDirectory =
    __SOURCE_DIRECTORY__ </> ".." </> ".."

let srcDirectory =
    rootDirectory </> "src"

let plugins =
    !! (srcDirectory </> "TransactionQL.Plugins.*/*proj")

let apps =
    !! (srcDirectory </> "TransactionQL.Console/*proj")
    ++ (srcDirectory </> "TransactionQL.DesktopApp/*proj")

let sln =
    rootDirectory </> "TransactionQL.sln"

let configuration () =
    match Environment.environVarOrDefault "CONFIGURATION" "Release" with
    | "Debug" -> DotNet.BuildConfiguration.Debug
    | "Release" -> DotNet.BuildConfiguration.Release
    | config -> DotNet.BuildConfiguration.Custom config

let version orDefault =
    Environment.environVarOrDefault "VERSION" orDefault 

/// Add a DotNet Build parameter to the list of custom parameters (-p).
let addParam key value param =
    let newParam = $"-p:{key}={value}"
    match param with
    | None -> Some newParam
    | Some prevParams -> Some $"{prevParams} {newParam}"

let withVersion ( v : string) (c : DotNet.PublishOptions) =
    { c with Common = { c.Common with CustomParams = addParam "Version" v c.Common.CustomParams } }

let appConfig (c : DotNet.PublishOptions) =
    { c with 
        Configuration = configuration ()
        Common = { c.Common with CustomParams = addParam "PublishSingleFile" "true" c.Common.CustomParams }
        SelfContained = Some false
    }

let initTargets () =
    Target.create "Clean" (fun _ ->
      Trace.log " --- Cleaning stuff --- "
      let dirs = 
        !! (srcDirectory </> "**" </> "bin") 
        ++ (srcDirectory </> "**" </> "obj")
        -- (srcDirectory </> "TransactionQL.Build" </> "bin")
        -- (srcDirectory </> "TransactionQL.Build" </> "obj")

      dirs 
      |> Shell.cleanDirs
    )

    Target.create "Test" (fun _ ->
      Trace.log " --- Testing the app --- "
      DotNet.test
          (fun c -> { c with Configuration = configuration () })
          sln
    )

    Target.create "Build" (fun _ ->
      Trace.log " --- Building the app --- "
      DotNet.build
          (fun c -> { c with Configuration = configuration () })
          sln
    )

    Target.create "Publish" (fun _ ->
      Trace.log " --- Publishing the app --- "
      let v = version "1.0.0"

      plugins |> (Seq.iter (fun (s : string) ->
        DotNet.publish (fun c -> c |> withVersion v) s))

      apps |> (Seq.iter (fun (s : string) -> 
        DotNet.publish (fun c -> appConfig c |> withVersion v) s) )
    )

    Target.create "Deploy" (fun _ ->
      Trace.log " --- Deploying app --- "
    )

    "Clean"
      //=?> ("Test", Environment.hasEnvironVar "VERSION")
      ==> "Test"
      ==> "Publish"
      //==> "Deploy"


[<EntryPoint>]
let main args =
    // *** Start Build ***
    args
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    initTargets ()
    |> ignore

    Target.runOrDefault "Publish"
    0
