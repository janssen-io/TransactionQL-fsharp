dotnet publish TransactionQL.Console/TransactionQL.Console.fsproj -r linux-x64 -p:PublishSingleFile=true --no-self-contained 
dotnet publish TransactionQL.Plugins.ING/TransactionQL.Plugins.ING.fsproj -r linux-x64 --no-self-contained -c Release
dotnet publish TransactionQL.Plugins.Bunq/TransactionQL.Plugins.Bunq.fsproj -r linux-x64 --no-self-contained -c Release
dotnet publish TransactionQL.Plugins.ASN/TransactionQL.Plugins.ASN.fsproj -r linux-x64 --no-self-contained -c Release
