param(
    [Parameter(Mandatory = $false)]
    [string]$EnvironmentUrl = "https://org1aa19a83.crm.dynamics.com",

    [Parameter(Mandatory = $false)]
    [string]$SolutionUniqueName = "nhspshiftexceptiondemo"
)

$ErrorActionPreference = "Stop"
$projectPath = Join-Path $PSScriptRoot "DataverseSchemaBuilder"

dotnet run --project $projectPath -- --environment-url $EnvironmentUrl --solution-unique-name $SolutionUniqueName