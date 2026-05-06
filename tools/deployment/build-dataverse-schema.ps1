param(
    [Parameter(Mandatory = $false)]
    [string]$EnvironmentUrl = "https://org1aa19a83.crm.dynamics.com",

    [Parameter(Mandatory = $false)]
    [string]$SolutionUniqueName = "nhspshiftexceptiondemo",

    [Parameter(Mandatory = $false)]
    [switch]$ImportSampleData,

    [Parameter(Mandatory = $false)]
    [string]$SamplesPath = (Join-Path $PSScriptRoot "..\..\samples")
)

$ErrorActionPreference = "Stop"
$projectPath = Join-Path $PSScriptRoot "DataverseSchemaBuilder"

$arguments = @(
    "--environment-url", $EnvironmentUrl,
    "--solution-unique-name", $SolutionUniqueName
)

if ($ImportSampleData) {
    $arguments += @("--import-sample-data", "true", "--samples-path", $SamplesPath)
}

dotnet run --project $projectPath -- $arguments