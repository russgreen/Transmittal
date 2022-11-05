@echo on
setlocal enableextensions disabledelayedexpansion

::Arguments of either PreBuild or PostBuild
set buildType=%1

::License key replacement file
set sourceFile=%2

::Text file containing license key
set keyFile=%3
	
::Replacement string
set DummyKey=##SyncfusionLicense##
::from .txt file
set /p LicenseKey=<"%keyFile%"

echo %LicenseKey%
	
::Replacement statement
if NOT "%LicenseKey%" == "" (
	if "%buildType%" == "PreBuild" (
		powershell -Command "(Get-Content %sourceFile%).Replace('%DummyKey%','%LicenseKey%')|Set-Content %sourceFile%"
	)
	if "%buildType%" == "PostBuild" (
		powershell -Command "(Get-Content %sourceFile%).Replace('%LicenseKey%','%DummyKey%')|Set-Content %sourceFile%"
	)
)