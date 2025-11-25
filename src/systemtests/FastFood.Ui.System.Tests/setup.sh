#!/bin/bash

# FastFood UI System Tests - Playwright Setup Script (Linux/macOS)
# This script installs Playwright browsers and dependencies

echo -e "\033[36mFastFood UI System Tests - Playwright Setup\033[0m"
echo -e "\033[36m============================================\033[0m"
echo ""

# Check if .NET is installed
echo -e "\033[33mChecking .NET installation...\033[0m"
dotnetVersion=$(dotnet --version 2>&1)
if [ $? -ne 0 ]; then
    echo -e "\033[31mERROR: .NET SDK not found. Please install .NET 9.0 or later.\033[0m"
    exit 1
fi
echo -e "\033[32m✓ .NET version: $dotnetVersion\033[0m"
echo ""

# Build the project
echo -e "\033[33mBuilding test project...\033[0m"
dotnet build
if [ $? -ne 0 ]; then
    echo -e "\033[31mERROR: Build failed.\033[0m"
    exit 1
fi
echo -e "\033[32m✓ Build successful\033[0m"
echo ""

# Install Playwright browsers
echo -e "\033[33mInstalling Playwright browsers...\033[0m"

# Check if pwsh (PowerShell Core) is available
if command -v pwsh &> /dev/null; then
    echo -e "\033[36mUsing PowerShell Core to run playwright.ps1...\033[0m"
    pwsh bin/Debug/net9.0/playwright.ps1 install chromium
    if [ $? -ne 0 ]; then
        echo -e "\033[31mERROR: Failed to install Playwright browsers.\033[0m"
        exit 1
    fi
    echo -e "\033[32m✓ Playwright browsers installed\033[0m"
else
    # Fallback to direct dotnet command
    echo -e "\033[36mPowerShell not found, using dotnet playwright...\033[0m"
    dotnet tool install --global Microsoft.Playwright.CLI 2>/dev/null || true
    playwright install chromium
    if [ $? -ne 0 ]; then
        echo -e "\033[31mERROR: Failed to install Playwright browsers.\033[0m"
        echo -e "\033[33mTry installing PowerShell Core: brew install powershell\033[0m"
        exit 1
    fi
    echo -e "\033[32m✓ Playwright browsers installed\033[0m"
fi

echo ""
echo -e "\033[36m============================================\033[0m"
echo -e "\033[32mSetup complete!\033[0m"
echo ""
echo -e "\033[33mNext steps:\033[0m"
echo -e "\033[37m1. Start the FastFood application (docker compose up)\033[0m"
echo -e "\033[37m2. Verify URLs are accessible:\033[0m"
echo -e "\033[90m   - https://pos.localtest.me/\033[0m"
echo -e "\033[90m   - https://kitchen.localtest.me/\033[0m"
echo -e "\033[90m   - https://orderstatus.localtest.me/\033[0m"
echo -e "\033[37m3. Run tests: dotnet test\033[0m"
echo ""
