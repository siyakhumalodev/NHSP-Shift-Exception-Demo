param(
    [Parameter(Mandatory = $false)]
    [string]$SourceDirectory = (Join-Path $PSScriptRoot "Src"),

    [Parameter(Mandatory = $false)]
    [string]$MsappOutputPath = (Join-Path (Join-Path $PSScriptRoot "..\..\..\out") "NHSPShiftExceptionManager.msapp")
)

$ErrorActionPreference = "Stop"

Write-Host "Checking NHSP Shift Exception Manager canvas source YAML..."

$python = @'
import pathlib
import sys
import yaml

source = pathlib.Path(sys.argv[1])
allowed_top_level = {"App", "Screens", "ComponentDefinitions", "DataSources", "EditorState"}
files = sorted(source.glob("*.pa.yaml"))

if not files:
    raise SystemExit(f"No .pa.yaml files found in {source}")

for path in files:
    with path.open("r", encoding="utf-8") as handle:
        data = yaml.safe_load(handle)

    if not isinstance(data, dict):
        raise SystemExit(f"{path} does not contain a YAML object at the document root")

    unknown_keys = set(data) - allowed_top_level
    if unknown_keys:
        raise SystemExit(f"{path} contains unsupported top-level keys: {', '.join(sorted(unknown_keys))}")

print(f"Validated {len(files)} canvas source YAML files.")
'@

python -c $python $SourceDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Canvas source YAML validation failed."
}

$manifestPath = Join-Path $PSScriptRoot "CanvasManifest.json"
if (Test-Path $manifestPath) {
    Write-Host "CanvasManifest.json found. Packing .msapp with PAC..."
    pac canvas pack --sources $PSScriptRoot --msapp $MsappOutputPath
    Write-Host "Packed canvas app: $MsappOutputPath"
}
else {
    Write-Host "Canvas source validation complete."
    Write-Host "No CanvasManifest.json is present, so this PAC version cannot pack an .msapp from the source-only app."
    Write-Host "Create or export a Studio app once, unpack it, then this source can be merged into that packable layout."
}
