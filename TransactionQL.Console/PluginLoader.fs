namespace TransactionQL.Console

open System
open System.IO
open System.Reflection
open TransactionQL.Input.Converters
open System.Runtime.Loader

module PluginLoader =

    let load pluginName pluginDirectory =
        let context = new AssemblyLoadContext("PluginLoader", true)
        (Path.GetFullPath pluginDirectory, pluginName)
        |> Path.Combine
        |> context.LoadFromAssemblyPath
        |> fun assembly -> assembly.GetTypes ()
        |> Array.tryFind (fun t -> (typeof<IConverter>).IsAssignableFrom t)
        |> Option.map (fun t -> Activator.CreateInstance t :?> IConverter)

