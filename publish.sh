dotnet publish TransactionQL.Console/TransactionQL.Console.fsproj -r linux-x64 -p:PublishSingleFile=true --self-contained=false
dotnet publish TransactionQL.Plugins.ING/TransactionQL.Plugins.ING.fsproj -r linux-x64
dotnet publish TransactionQL.Plugins.Bunq/TransactionQL.Plugins.Bunq.fsproj -r linux-x64
