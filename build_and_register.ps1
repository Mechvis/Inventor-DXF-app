# PowerShell script for building and registering the Sheet Metal DXF Exporter
# Run as Administrator

param(
    [switch]$Build = $true,
    [switch]$Register = $true,
    [string]$Configuration = "Release"
)

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Sheet Metal DXF Exporter - Build & Registration" -ForegroundColor Cyan  
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host

# Check if running as administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Set paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionFile = Join-Path $scriptDir "SheetMetalDXFExporter.sln"
$dllPath = Join-Path $scriptDir "bin\$Configuration\SheetMetalDXFExporter.dll"

Write-Host "Script Directory: $scriptDir" -ForegroundColor Gray
Write-Host "Solution File: $solutionFile" -ForegroundColor Gray
Write-Host "Target DLL: $dllPath" -ForegroundColor Gray
Write-Host

# Build project if requested
if ($Build) {
    Write-Host "Building solution..." -ForegroundColor Yellow
    
    if (-not (Test-Path $solutionFile)) {
        Write-Host "ERROR: Solution file not found: $solutionFile" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    try {
        & dotnet build "$solutionFile" --configuration $Configuration --verbosity minimal
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Build completed successfully!" -ForegroundColor Green
        } else {
            Write-Host "Build failed with exit code: $LASTEXITCODE" -ForegroundColor Red
            
            # Try MSBuild as fallback
            Write-Host "Trying MSBuild..." -ForegroundColor Yellow
            & msbuild "$solutionFile" /p:Configuration=$Configuration /verbosity:minimal
            
            if ($LASTEXITCODE -ne 0) {
                Write-Host "MSBuild also failed. Please check build errors." -ForegroundColor Red
                Read-Host "Press Enter to exit"  
                exit 1
            }
        }
    }
    catch {
        Write-Host "Build error: $($_.Exception.Message)" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
}

# Register COM add-in if requested
if ($Register) {
    Write-Host "Registering COM Add-in..." -ForegroundColor Yellow
    
    if (-not (Test-Path $dllPath)) {
        Write-Host "ERROR: DLL not found at $dllPath" -ForegroundColor Red
        Write-Host "Please ensure the build completed successfully." -ForegroundColor Yellow
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    try {
        & regasm "$dllPath" /codebase
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host
            Write-Host "SUCCESS: Add-in registered successfully!" -ForegroundColor Green
            Write-Host
            Write-Host "Next steps:" -ForegroundColor Cyan
            Write-Host "1. Start Autodesk Inventor 2026" -ForegroundColor White
            Write-Host "2. Look for 'Sheet Metal DXF Export' button in Tools ribbon" -ForegroundColor White
            Write-Host "3. Check Add-ins dialog if button doesn't appear" -ForegroundColor White
            Write-Host
        } else {
            Write-Host "Registration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "Registration error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Check Inventor installation
Write-Host "Checking Inventor installation..." -ForegroundColor Yellow
$inventorPath = "C:\Program Files\Autodesk\Inventor 2026\Bin\Inventor.exe"
if (Test-Path $inventorPath) {
    Write-Host "✓ Inventor 2026 found at: $inventorPath" -ForegroundColor Green
} else {
    Write-Host "⚠ Inventor 2026 not found at expected location" -ForegroundColor Yellow
    Write-Host "  Please verify Inventor 2026 is installed" -ForegroundColor Gray
}

Write-Host
Write-Host "Operation completed." -ForegroundColor Cyan
Read-Host "Press Enter to exit"