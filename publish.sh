dotnet publish TransactionQL.Console/TransactionQL.Console.fsproj -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true
dotnet publish TransactionQL.Plugins.ING/TransactionQL.Plugins.ING.fsproj -r linux-x64
dotnet publish TransactionQL.Plugins.Bunq/TransactionQL.Plugins.Bunq.fsproj -r linux-x64
