param(
    [string]$Configuration,
    [string]$ProjectPath,
    [string]$Target = "msbuild"  # or "dotnet"
)

$ProjectPath = $ProjectPath -replace '^\.\.', $PSScriptRoot
Write-Host "Building $ProjectPath ($Configuration) using $Target..."

# Clear MSBuildExtensionsPath to avoid SDK conflict when using Mono
$env:MSBuildExtensionsPath = $null

# Determine null device
$nullDevice = if ($IsWindows) { 'NUL' } else { '/dev/null' }

# Prepare argument list based on target
$arguments = if ($Target -ieq "dotnet") {
    @("build", "`"$ProjectPath`"", "-c", $Configuration)
}
else {
    @("`"$ProjectPath`"", "/t:Rebuild", "/p:Configuration=$Configuration")
}

# Launch build process
$processInfo = @{
    FilePath               = $Target
    ArgumentList           = $arguments
    RedirectStandardOutput = $nullDevice
    PassThru               = $true
    NoNewWindow            = $true
}

$proc = Start-Process @processInfo
$proc.WaitForExit()

if ($proc.ExitCode -eq 0) {
    Write-Host "Build succeeded."
}
else {
    Write-Host "Build failed with exit code $($proc.ExitCode)"
    exit $proc.ExitCode
}
