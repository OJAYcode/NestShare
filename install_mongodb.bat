@echo off
echo MongoDB Installation Helper
echo =========================
echo.
echo This script will help you download and install MongoDB on your system.
echo.

REM Check if MongoDB is already installed
mongod --version > nul 2>&1
if %errorlevel% == 0 (
    echo MongoDB is already installed on your system!
    mongod --version
    echo.
    echo You need to ensure the MongoDB service is running.
    echo.
    goto :end
)

echo MongoDB is not installed or not in your PATH.
echo.
echo Would you like to download MongoDB Community Edition installer?
choice /C YN /M "Download MongoDB installer?"
if errorlevel 2 goto :no_download
if errorlevel 1 goto :download

:download
echo.
echo Downloading MongoDB installer...
echo.
echo Opening MongoDB download page in your browser...
start https://www.mongodb.com/try/download/community
echo.
echo Please download the MSI installer for Windows and run it.
echo.
echo Installation instructions:
echo 1. Choose "Complete" installation
echo 2. Ensure "Install MongoDB as a Service" is checked
echo 3. Complete the installation
echo.
echo After installation, MongoDB service should start automatically.
echo.
echo After installing, please:
echo 1. Update your connection string in appsettings.json if needed
echo 2. Restart your application
goto :end

:no_download
echo.
echo Installation canceled.
echo.
echo You'll need to install MongoDB manually to use the database features.
echo.

:end
echo Press any key to exit...
pause > nul
