$ErrorActionPreference = "Stop"

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    throw "Compiler C# (.NET Framework) tidak ditemukan pada: $csc"
}

$distDir = Join-Path $PSScriptRoot "dist"
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir | Out-Null
}

$manifest = Join-Path $PSScriptRoot "client-src\app.manifest"
$clientSrc = Join-Path $PSScriptRoot "client-src\Program.cs"
$installerSrc = Join-Path $PSScriptRoot "client-src\Installer.cs"

$clientExe = Join-Path $distDir "AIBlockerGuard.exe"
$setupExe = Join-Path $distDir "AIBlockerSetup.exe"

Write-Host "[1/2] Mengompilasi Aplikasi Client PC Mahasiswa: AIBlockerGuard.exe ..." -ForegroundColor Cyan
& $csc /target:winexe /out:$clientExe /win32manifest:$manifest /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /optimize+ $clientSrc
if ($LASTEXITCODE -ne 0) {
    throw "Kompilasi AIBlockerGuard.exe gagal!"
}
Write-Host "      Berhasil dibuat: $clientExe ($((Get-Item $clientExe).Length) bytes)" -ForegroundColor Green

Write-Host "[2/2] Mengompilasi Installer Setup: AIBlockerSetup.exe (Memuat payload AIBlockerGuard.exe) ..." -ForegroundColor Cyan
$resArg = "/resource:$clientExe,AIBlockerGuard.exe"
& $csc /target:winexe /out:$setupExe /win32manifest:$manifest /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll $resArg /optimize+ $installerSrc
if ($LASTEXITCODE -ne 0) {
    throw "Kompilasi AIBlockerSetup.exe gagal!"
}
Write-Host "      Berhasil dibuat: $setupExe ($((Get-Item $setupExe).Length) bytes)" -ForegroundColor Green

Write-Host "`nSeluruh paket distribusi siap digunakan di folder dist\ !" -ForegroundColor Green
