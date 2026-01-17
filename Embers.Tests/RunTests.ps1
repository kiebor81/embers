# Run Embers.Tests with timestamped results export
# Results are saved to .vs/ directory

param(
    [string]$Configuration = "Debug",
    [string]$Filter = ""
)

$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$resultsDir = Join-Path $PSScriptRoot "..\.vs"
$projectPath = Join-Path $PSScriptRoot "Embers.Tests.csproj"

Write-Host "Running tests at $timestamp..." -ForegroundColor Cyan
Write-Host "Results will be saved to: $resultsDir" -ForegroundColor Yellow

# Build the dotnet test command
$testCommand = "dotnet test `"$projectPath`" --configuration $Configuration"

# Add logger for TRX format
$testCommand += " --logger `"trx;LogFileName=${timestamp}_testOutcomes.trx`""

# Add logger for HTML format
$testCommand += " --logger `"html;LogFileName=${timestamp}_testOutcomes.html`""

# Set results directory
$testCommand += " --results-directory `"$resultsDir`""

# Add filter if specified
if ($Filter) {
    $testCommand += " --filter `"$Filter`""
    Write-Host "Filter: $Filter" -ForegroundColor Yellow
}

# Add console logger with detailed output
$testCommand += " --logger `"console;verbosity=normal`""

Write-Host "`nExecuting: $testCommand" -ForegroundColor Gray
Write-Host ""

# Execute the command
Invoke-Expression $testCommand

# Display results location
Write-Host "`n" -NoNewline
Write-Host "Test results saved:" -ForegroundColor Green
Write-Host "  TRX:  $resultsDir\${timestamp}_testOutcomes.trx" -ForegroundColor White
Write-Host "  HTML: $resultsDir\${timestamp}_testOutcomes.html" -ForegroundColor White

# Return exit code
$exitCode = $LASTEXITCODE
exit $exitCode
