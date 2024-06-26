param([string]$configuration="Release") 

function AddToPath($dir) {
    if ($env:PATH.Contains($dir)) { return }

    $addToPath = Read-Host "Add '$dir' to path? [y/n]"
    if ($addToPath -eq "y") {
        [System.Environment]::SetEnvironmentVariable('PATH',"$env:PATH;$dir", 'User')
        $env:PATH = [System.Environment]::GetEnvironmentVariable('PATH')
    }
}


dotnet publish TransactionQL.Plugins.ING/TransactionQL.Plugins.ING.fsproj -c $configuration
dotnet publish TransactionQL.Plugins.Bunq/TransactionQL.Plugins.Bunq.fsproj -c $configuration
dotnet publish TransactionQL.Plugins.ASN/TransactionQL.Plugins.ASN.fsproj -c $configuration
dotnet publish TransactionQL.Console\TransactionQL.Console.fsproj -c $configuration -r win-x64 -p:PublishSingleFile=true --no-self-contained
dotnet publish TransactionQL.DesktopApp/TransactionQL.DesktopApp.csproj -c $configuration -r win-x64 -p:PublishSingleFile=true --no-self-contained


$appData = [environment]::GetFolderPath("ApplicationData")
$appDir = Join-Path $appData "tql"
$pluginDir = Join-Path $appDir "plugins"
New-Item -Path $appDir -ItemType Directory -ErrorAction SilentlyContinue
New-Item -Path $pluginDir -ItemType Directory -ErrorAction SilentlyContinue

Write-Host "Installing TQL to $appDir"

Write-Host "- tql.exe"
Copy-Item -Path ".\TransactionQL.Console\bin\$configuration\net8.0\win-x64\publish\TransactionQL.Console.exe" -Destination "$appDir\tql.exe" -Force

Write-Host "- plugins/ing"
Copy-Item -Path ".\TransactionQL.Plugins.ING\bin\$configuration\net8.0\publish\TransactionQL.Plugins.ING.dll" -Destination "$pluginDir\ing.dll" -Force

Write-Host "- plugins/asn"
Copy-Item -Path ".\TransactionQL.Plugins.ASN\bin\$configuration\net8.0\publish\TransactionQL.Plugins.ASN.dll" -Destination "$pluginDir\asn.dll" -Force

Write-Host "- plugins/bunq"
Copy-Item -Path ".\TransactionQL.Plugins.Bunq\bin\$configuration\net8.0\publish\TransactionQL.Plugins.Bunq.dll" -Destination "$pluginDir\bunq.dll" -Force

Write-Host "- Desktop App"
mkdir "$appdir\app" -Force
Copy-Item -Path ".\TransactionQL.DesktopApp\bin\$configuration\net8.0\win-x64\publish\*" -Destination "$appdir\app" -Recurse -Force

AddToPath $appDir
AddToPath (Join-Path $appDir "app")

explorer.exe "$appdir\app"
