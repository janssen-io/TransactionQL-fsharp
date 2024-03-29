param([string]$configuration="Release") 

dotnet publish TransactionQL.Console\TransactionQL.Console.fsproj -c $configuration -r win-x64 -p:PublishSingleFile=true --no-self-contained
dotnet publish TransactionQL.Plugins.ING/TransactionQL.Plugins.ING.fsproj -r win-x64 -c $configuration --no-self-contained
dotnet publish TransactionQL.Plugins.Bunq/TransactionQL.Plugins.Bunq.fsproj -r win-x64 -c $configuration --no-self-contained
dotnet publish TransactionQL.Plugins.ASN/TransactionQL.Plugins.ASN.fsproj -r win-x64 -c $configuration --no-self-contained

$appData = [environment]::GetFolderPath("ApplicationData")
$appDir = "$appData\tql"
$pluginDir = "$appData\tql\plugins"
New-Item -Path $appDir -ItemType Directory -ErrorAction SilentlyContinue
New-Item -Path $pluginDir -ItemType Directory -ErrorAction SilentlyContinue

Write-Host "Installing TQL to $appDir"

Write-Host "- tql.exe"
Copy-Item -Path ".\TransactionQL.Console\bin\$configuration\net6.0\win-x64\publish\TransactionQL.Console.exe" -Destination "$appDir\tql.exe" -Force

Write-Host "- plugins/ing"
Copy-Item -Path ".\TransactionQL.Plugins.ING\bin\$configuration\net6.0\win-x64\publish\TransactionQL.Plugins.ING.dll" -Destination "$pluginDir\ing" -Force

Write-Host "- plugins/asn"
Copy-Item -Path ".\TransactionQL.Plugins.ASN\bin\$configuration\net6.0\win-x64\publish\TransactionQL.Plugins.ASN.dll" -Destination "$pluginDir\asn" -Force

Write-Host "- plugins/bunq"
Copy-Item -Path ".\TransactionQL.Plugins.Bunq\bin\$configuration\net6.0\win-x64\publish\TransactionQL.Plugins.Bunq.dll" -Destination "$pluginDir\bunq" -Force
