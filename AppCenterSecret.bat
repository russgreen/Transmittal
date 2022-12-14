@echo on
setlocal enableextensions disabledelayedexpansion

::Arguments of either PreBuild or PostBuild
set buildType=%1

::License key replacement file
set sourceFile=%2

::Text file containing AppCenter.MS secret key
set secretFile=%3
	
::Replacement string
set DummySecret=##AppCenterSecret##
::from .txt file
set /p AppCenterSecret=<"%secretFile%"

echo %AppCenterSecret%
	
::Replacement statement
if NOT "%AppCenterSecret%" == "" (
	if "%buildType%" == "PreBuild" (
		powershell -Command "(Get-Content %sourceFile%).Replace('%DummySecret%','%AppCenterSecret%')|Set-Content %sourceFile%"
	)
	if "%buildType%" == "PostBuild" (
		powershell -Command "(Get-Content %sourceFile%).Replace('%AppCenterSecret%','%DummySecret%')|Set-Content %sourceFile%"
	)
)