param(
    [Parameter(Mandatory = $false)]
    [string]$EnvironmentUrl = "https://org1aa19a83.crm.dynamics.com",

    [Parameter(Mandatory = $false)]
    [string]$SolutionUniqueName = "nhspshiftexceptiondemo",

    [Parameter(Mandatory = $false)]
    [string]$FlowClientDataPath = (Join-Path $PSScriptRoot "..\..\powerautomate\flows\NHSPEscalateShiftException\clientdata.json")
)

$ErrorActionPreference = "Stop"
$projectPath = Join-Path $PSScriptRoot "DataverseSchemaBuilder"

$arguments = @(
    "--environment-url", $EnvironmentUrl,
    "--solution-unique-name", $SolutionUniqueName,
    "--deploy-flow", "true",
    "--flow-clientdata-path", $FlowClientDataPath
)

dotnet run --project $projectPath -- $arguments
