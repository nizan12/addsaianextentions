$ErrorActionPreference = "Stop"

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    throw "Compiler C# (.NET Framework) tidak ditemukan pada: $csc"
}

$distDir = Join-Path $PSScriptRoot "dist"
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir | Out-Null
}

$appManifest = Join-Path $PSScriptRoot "client-src\app.manifest"
$setupManifest = Join-Path $PSScriptRoot "client-src\setup.manifest"
$serviceSrc = Join-Path $PSScriptRoot "client-src\Service.cs"
$clientSrc = Join-Path $PSScriptRoot "client-src\Program.cs"
$installerSrc = Join-Path $PSScriptRoot "client-src\Installer.cs"

$serviceExe = Join-Path $distDir "AIBlockerService.exe"
$clientExe = Join-Path $distDir "AIBlockerGuard.exe"
$setupExe = Join-Path $distDir "AIBlockerSetup.exe"

Write-Host "[1/3] Mengompilasi Windows Service: AIBlockerService.exe ..." -ForegroundColor Cyan
& $csc /target:exe /out:$serviceExe /r:System.dll /r:System.Core.dll /r:System.ServiceProcess.dll /r:System.Web.Extensions.dll /optimize+ $serviceSrc
if ($LASTEXITCODE -ne 0) {
    throw "Kompilasi AIBlockerService.exe gagal!"
}
Write-Host "      Berhasil dibuat: $serviceExe ($((Get-Item $serviceExe).Length) bytes)" -ForegroundColor Green

Write-Host "[2/3] Mengompilasi Aplikasi Client PC Mahasiswa (asInvoker): AIBlockerGuard.exe ..." -ForegroundColor Cyan
& $csc /target:winexe /out:$clientExe /win32manifest:$appManifest /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Management.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /r:System.ServiceProcess.dll /optimize+ $clientSrc
if ($LASTEXITCODE -ne 0) {
    throw "Kompilasi AIBlockerGuard.exe gagal!"
}
Write-Host "      Berhasil dibuat: $clientExe ($((Get-Item $clientExe).Length) bytes)" -ForegroundColor Green

Write-Host "[3/3] Mengompilasi Installer Setup (Admin): AIBlockerSetup.exe ..." -ForegroundColor Cyan
$resClientArg = "/resource:$clientExe,AIBlockerGuard.exe"
$resServiceArg = "/resource:$serviceExe,AIBlockerService.exe"
& $csc /target:winexe /out:$setupExe /win32manifest:$setupManifest /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.ServiceProcess.dll $resClientArg $resServiceArg /optimize+ $installerSrc
if ($LASTEXITCODE -ne 0) {
    throw "Kompilasi AIBlockerSetup.exe gagal!"
}
Write-Host "      Berhasil dibuat: $setupExe ($((Get-Item $setupExe).Length) bytes)" -ForegroundColor Green

Write-Host "`nSeluruh paket distribusi siap digunakan di folder dist\ !" -ForegroundColor Green
