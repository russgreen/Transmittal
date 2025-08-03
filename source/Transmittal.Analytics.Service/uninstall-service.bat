@echo off
echo Uninstalling Transmittal Analytics Service...

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Administrator rights confirmed.
) else (
    echo This script requires administrator privileges.
    echo Please run as administrator.
    pause
    exit /b 1
)

REM Stop the service
echo Stopping service...
sc stop "TransmittalAnalyticsService"

REM Wait a moment for the service to stop
timeout /t 3 /nobreak >nul

REM Delete the service
echo Removing service...
sc delete "TransmittalAnalyticsService"

if %errorLevel% == 0 (
    echo Service uninstalled successfully.
) else (
    echo ERROR: Failed to uninstall service.
    echo Error code: %errorLevel%
    echo The service may not be installed or may still be running.
)

echo.
pause