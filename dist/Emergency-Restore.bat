@echo off
title AI-Blocker Guard — Pemulihan Sistem Darurat
echo ====================================================================
echo   AI-BLOCKER GUARD - PEMULIHAN SISTEM DARURAT (EMERGENCY RESTORE)
echo ====================================================================
echo.

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [!] PERINGATAN: Klik kanan pada file ini dan pilih "Run as administrator"!
    pause
    exit /b 1
)

echo [*] Menghentikan proses AIBlockerGuard jika sedang berjalan...
taskkill /f /im AIBlockerGuard.exe >nul 2>&1

if exist "%~dp0AIBlockerGuard.exe" (
    echo [*] Menjalankan pemulihan otomatis lewat AIBlockerGuard.exe --restore ...
    "%~dp0AIBlockerGuard.exe" --restore
)

if exist "C:\Program Files\AIBlockerGuard\AIBlockerGuard.exe" (
    echo [*] Menjalankan pemulihan otomatis lewat Program Files ...
    "C:\Program Files\AIBlockerGuard\AIBlockerGuard.exe" --restore
)

echo [*] Membersihkan kebijakan Registry Browser URLBlocklist, URLAllowlist, DoH & VS Code...
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome\URLBlocklist" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome\URLBlocklist" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome\URLAllowlist" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome\URLAllowlist" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v DnsOverHttpsMode /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v BuiltInDnsClientEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v QuicAllowed /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v GenAiDefaultSettings /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v AIModeSettings /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v GeminiSettings /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v GenAILocalFoundationalModelSettings /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v DefaultSearchProviderEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v DefaultSearchProviderName /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v DefaultSearchProviderKeyword /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Google\Chrome" /v DefaultSearchProviderSearchURL /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v DnsOverHttpsMode /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v BuiltInDnsClientEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v QuicAllowed /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v GenAiDefaultSettings /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v AIModeSettings /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v GeminiSettings /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v GenAILocalFoundationalModelSettings /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v DefaultSearchProviderEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v DefaultSearchProviderName /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v DefaultSearchProviderKeyword /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Google\Chrome" /v DefaultSearchProviderSearchURL /f >nul 2>&1

reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge\URLBlocklist" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge\URLBlocklist" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge\URLAllowlist" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge\URLAllowlist" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v DnsOverHttpsMode /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v BuiltInDnsClientEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v QuicAllowed /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v HubsSidebarEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v CopilotCDPPageContext /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v CopilotPageContext /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v EdgeEntraCopilotPageContext /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v DiscoverPageContextEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v DefaultSearchProviderEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v DefaultSearchProviderName /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v DefaultSearchProviderKeyword /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v DefaultSearchProviderSearchURL /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v DnsOverHttpsMode /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v BuiltInDnsClientEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v QuicAllowed /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v HubsSidebarEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v CopilotCDPPageContext /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v CopilotPageContext /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v EdgeEntraCopilotPageContext /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v DiscoverPageContextEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v DefaultSearchProviderEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v DefaultSearchProviderName /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v DefaultSearchProviderKeyword /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\Edge" /v DefaultSearchProviderSearchURL /f >nul 2>&1

reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave\URLBlocklist" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave\URLBlocklist" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave\URLAllowlist" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave\URLAllowlist" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave" /v DnsOverHttpsMode /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave" /v BuiltInDnsClientEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave" /v QuicAllowed /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave" /v BraveAIChatEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave" /v DefaultSearchProviderEnabled /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave" /v DefaultSearchProviderName /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave" /v DefaultSearchProviderKeyword /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\BraveSoftware\Brave" /v DefaultSearchProviderSearchURL /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave" /v DnsOverHttpsMode /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave" /v BuiltInDnsClientEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave" /v QuicAllowed /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave" /v BraveAIChatEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave" /v DefaultSearchProviderEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave" /v DefaultSearchProviderName /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave" /v DefaultSearchProviderKeyword /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\BraveSoftware\Brave" /v DefaultSearchProviderSearchURL /f >nul 2>&1

reg delete "HKLM\SOFTWARE\Policies\Microsoft\VSCode" /v ChatEnabled /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Policies\Microsoft\VSCode" /v ChatEnabled /f >nul 2>&1

if exist "C:\ProgramData\AIBlockerGuard\vscode_settings_backup.json" (
    echo [*] Memulihkan settings.json VS Code asli...
    copy /Y "C:\ProgramData\AIBlockerGuard\vscode_settings_backup.json" "%APPDATA%\Code\User\settings.json" >nul 2>&1
    del /F /Q "C:\ProgramData\AIBlockerGuard\vscode_settings_backup.json" >nul 2>&1
)

echo [*] Melakukan Flush DNS Windows...
ipconfig /flushdns >nul

echo.
echo [✓] Selesai! Sistem PC telah dipulihkan ke kondisi normal.
pause
