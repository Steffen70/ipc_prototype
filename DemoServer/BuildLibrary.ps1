param(
    [string]$Configuration
)

$projectPath = Resolve-Path "$PSScriptRoot/../DemoLibrary/DemoLibrary.csproj"

Write-Host "Building $projectPath ($Configuration)..."

# Clear MSBuildExtensionsPath to avoid .NET SDK conflicts with Mono
$env:MSBuildExtensionsPath = $null

# Synchronously run msbuild and suppress output
$processInfo = @{
    FilePath               = "msbuild"
    ArgumentList           = $projectPath, "/t:Rebuild /p:Configuration=$Configuration"
    RedirectStandardOutput = "/dev/null"
    PassThru               = $true
    NoNewWindow            = $true
}

$proc = Start-Process @processInfo

# Wait for the process to finish
$proc.WaitForExit()

if ($proc.ExitCode -eq 0) {
    Write-Host "Build succeeded."
}
else {
    Write-Host "Build failed with exit code $($proc.ExitCode)"
    exit $proc.ExitCode
}
