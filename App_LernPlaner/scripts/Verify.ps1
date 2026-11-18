[CmdletBinding()]
param([switch]$SkipSql)
$ErrorActionPreference = 'Stop'
$previousSkip = $env:LERNPLANER_SKIP_SQL_TESTS
Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    if ($SkipSql) { $env:LERNPLANER_SKIP_SQL_TESTS = '1' }
    else { Remove-Item Env:LERNPLANER_SKIP_SQL_TESTS -ErrorAction SilentlyContinue }
    dotnet restore LernPlaner.sln --configfile NuGet.Config --packages artifacts/packages
    if ($LASTEXITCODE -ne 0) { throw 'Dependency restore failed.' }
    dotnet build LernPlaner.sln --no-restore --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    dotnet test tests/LernPlaner.Tests/LernPlaner.Tests.csproj --no-build --no-restore --logger 'trx;LogFileName=verification.trx' --results-directory artifacts/TestResults --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Automated tests failed.' }
}
finally {
    if ($null -eq $previousSkip) { Remove-Item Env:LERNPLANER_SKIP_SQL_TESTS -ErrorAction SilentlyContinue }
    else { $env:LERNPLANER_SKIP_SQL_TESTS = $previousSkip }
    Pop-Location
}
