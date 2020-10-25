dotnet publish TransactionQL.Console\TransactionQL.Console.fsproj -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained=true
dotnet publish TransactionQL.Plugins.ING/TransactionQL.Plugins.ING.fsproj -r win-x64 -c Release
dotnet publish TransactionQL.Plugins.Bunq/TransactionQL.Plugins.Bunq.fsproj -r win-x64 -c Release
