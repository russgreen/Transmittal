@echo off
echo Installing Transmittal Analytics Service...

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

REM Get the directory where this script is located
set "SERVICE_DIR=%~dp0"
set "SERVICE_EXE=%SERVICE_DIR%Transmittal.Analytics.Service.exe"

echo Service directory: %SERVICE_DIR%
echo Service executable: %SERVICE_EXE%

REM Check if the service executable exists
if not exist "%SERVICE_EXE%" (
    echo ERROR: Service executable not found at %SERVICE_EXE%
    echo Please ensure the service has been built and published.
    pause
    exit /b 1
)

REM Stop the service if it's running
echo Stopping existing service...
sc stop "TransmittalAnalyticsService" >nul 2>&1

REM Delete existing service
echo Removing existing service...
sc delete "TransmittalAnalyticsService" >nul 2>&1

REM Install the new service
echo Installing service...
sc create "TransmittalAnalyticsService" binPath= "\"%SERVICE_EXE%\"" start= auto DisplayName= "Transmittal Analytics Service" Description= "Collects and forwards analytics data for the Transmittal application"

if %errorLevel% == 0 (
    echo Service installed successfully.
    
    REM Start the service
    echo Starting service...
    sc start "TransmittalAnalyticsService"
    
    if %errorLevel% == 0 (
        echo Service started successfully.
        echo.
        echo The Transmittal Analytics Service has been installed and started.
        echo You can manage it through Windows Services (services.msc).
    ) else (
        echo WARNING: Service installed but failed to start.
        echo Check the Event Log for details.
    )
) else (
    echo ERROR: Failed to install service.
    echo Error code: %errorLevel%
)

echo.
pause