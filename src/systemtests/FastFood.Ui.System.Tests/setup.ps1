#!/usr/bin/env pwsh

# FastFood UI System Tests - Playwright Setup Script
# This script installs Playwright browsers and dependencies

Write-Host "FastFood UI System Tests - Playwright Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET is installed
Write-Host "Checking .NET installation..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 9.0 or later." -ForegroundColor Red
    exit 1
}
Write-Host "✓ .NET version: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Build the project
Write-Host "Building test project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green
Write-Host ""

# Install Playwright browsers
Write-Host "Installing Playwright browsers..." -ForegroundColor Yellow
$playwrightScript = "bin/Debug/net9.0/playwright.ps1"

if (Test-Path $playwrightScript) {
    & $playwrightScript install chromium
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install Playwright browsers." -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Playwright browsers installed" -ForegroundColor Green
} else {
    Write-Host "ERROR: Playwright script not found. Make sure the project is built." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Start the FastFood application (docker compose up)" -ForegroundColor White
Write-Host "2. Verify URLs are accessible:" -ForegroundColor White
Write-Host "   - https://pos.localtest.me/" -ForegroundColor Gray
Write-Host "   - https://kitchen.localtest.me/" -ForegroundColor Gray
Write-Host "   - https://orderstatus.localtest.me/" -ForegroundColor Gray
Write-Host "3. Run tests: dotnet test" -ForegroundColor White
Write-Host ""
