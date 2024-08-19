namespace TransactionQL.Application

open System.Diagnostics
open System.Reflection

module Configuration =

    open System
    open System.IO

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

    let getAppVersion =
        let assembly = Assembly.GetExecutingAssembly()
        let fileVersionInfo = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        let v = fileVersionInfo.InformationalVersion
        let i = v.LastIndexOf("+")
        if i < 0
            then v
            else v.Substring(0, i)
