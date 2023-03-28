namespace TransactionQL.Application

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
