param([string]$configuration="Release", [bool]$UseLatestTagVersion=$true) 

function AddToPath($dir) {
    if ($env:PATH.Contains($dir)) { return }

    $addToPath = Read-Host "Add '$dir' to path? [y/n]"
    if ($addToPath -eq "y") {
        [System.Environment]::SetEnvironmentVariable('PATH',"$env:PATH;$dir", 'User')
        $env:PATH = [System.Environment]::GetEnvironmentVariable('PATH')
    }
}

if ($UseLatestTagVersion){
    $latest_tag=$(git tag | sort | % { $_.Substring(1) } | Select -Last 1 )
    $commit=$(git rev-parse --short HEAD)
    $version="$($latest_tag)-$commit"
}
else {
    $version="1.0.0"
}
Write-Host "Publishing version > $version <"

dotnet publish TransactionQL.Plugins.ING/TransactionQL.Plugins.ING.fsproj -c $configuration -p:Version=$version
dotnet publish TransactionQL.Plugins.Bunq/TransactionQL.Plugins.Bunq.fsproj -c $configuration -p:Version=$version
dotnet publish TransactionQL.Plugins.ASN/TransactionQL.Plugins.ASN.fsproj -c $configuration -p:Version=$version
dotnet publish TransactionQL.Console\TransactionQL.Console.fsproj -c $configuration -r win-x64 -p:PublishSingleFile=true --no-self-contained -p:Version=$version
dotnet publish TransactionQL.DesktopApp/TransactionQL.DesktopApp.csproj -c $configuration -r win-x64 -p:PublishSingleFile=true --no-self-contained -p:Version=$version

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
