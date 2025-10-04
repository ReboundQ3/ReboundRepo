# Test script voor batch analyse
Write-Host "🧪 Testing Phishing Detector - Batch Analysis" -ForegroundColor Cyan
Write-Host ""

$testEmailsPath = "D:\Visual Studio Code\ReboundRepo\PhishingDetector\data\test_emails"

# Test of de directory bestaat
if (-not (Test-Path $testEmailsPath)) {
    Write-Host "❌ Test emails directory not found!" -ForegroundColor Red
    exit 1
}

Write-Host "📂 Test emails directory found: $testEmailsPath" -ForegroundColor Green
Write-Host ""

# Laat structuur zien
Write-Host "📁 Directory structure:" -ForegroundColor Yellow
Get-ChildItem -Path $testEmailsPath -Recurse | Select-Object FullName

Write-Host ""
Write-Host "Press any key to start batch analysis..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Run de applicatie met menu optie 2 (Batch Analysis)
cd "D:\Visual Studio Code\ReboundRepo\PhishingDetector\src\PhishingDetector.App"
Write-Host ""
Write-Host "Starting application..." -ForegroundColor Green
Write-Host "Select option 2 for Batch Analysis" -ForegroundColor Yellow
Write-Host "Then enter path: $testEmailsPath" -ForegroundColor Yellow
Write-Host ""

dotnet run
