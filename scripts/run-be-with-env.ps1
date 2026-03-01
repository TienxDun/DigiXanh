[CmdletBinding()]
param(
    [string]$ProjectPath = "DigiXanh.API",
    [string]$EnvFile = "DigiXanh.API/.env",
    [string]$LaunchProfile = "https",
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$DotnetArgs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectFullPath = Join-Path $repoRoot $ProjectPath
$envFullPath = Join-Path $repoRoot $EnvFile

if (-not (Test-Path $projectFullPath)) {
    throw "Không tìm thấy thư mục project: $projectFullPath"
}

if (-not (Test-Path $envFullPath)) {
    throw "Không tìm thấy file .env: $envFullPath"
}

Write-Host "[1/3] Loading env file: $envFullPath" -ForegroundColor Cyan

Get-Content $envFullPath | ForEach-Object {
    $line = $_.Trim()

    if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('#')) {
        return
    }

    $parts = $line -split '=', 2
    if ($parts.Count -ne 2) {
        Write-Warning "Bỏ qua dòng không hợp lệ: $line"
        return
    }

    $key = $parts[0].Trim()
    $value = $parts[1].Trim()

    if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
        $value = $value.Substring(1, $value.Length - 2)
    }

    [System.Environment]::SetEnvironmentVariable($key, $value, 'Process')
}

Write-Host "[2/3] Environment variables loaded for current process." -ForegroundColor Green

Push-Location $projectFullPath
try {
    Write-Host "[3/3] Starting backend: dotnet run --launch-profile $LaunchProfile" -ForegroundColor Cyan

    $runArgs = @('run', '--launch-profile', $LaunchProfile)
    if ($DotnetArgs -and $DotnetArgs.Count -gt 0) {
        $runArgs += $DotnetArgs
    }

    & dotnet @runArgs
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
