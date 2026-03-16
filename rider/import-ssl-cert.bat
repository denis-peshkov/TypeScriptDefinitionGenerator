@echo off
rem Import a CA certificate into Java cacerts so Gradle can use HTTPS (e.g. plugins.gradle.org).
rem Run this script as Administrator (right-click cmd -> Run as administrator).
rem Usage: import-ssl-cert.bat [path-to-cert.cer]
rem Example: import-ssl-cert.bat C:\Users\You\gradle-org.cer

setlocal
set "CER_FILE=%~1"
if "%CER_FILE%"=="" (
    echo Usage: %~nx0 path-to-cert.cer
    echo Example: %~nx0 gradle-org.cer
    echo.
    echo Run cmd as Administrator, then run this script with the full path to your .cer file.
    exit /b 1
)

if not exist "%CER_FILE%" (
    echo Error: File not found: %CER_FILE%
    exit /b 1
)

if not defined JAVA_HOME (
    echo Error: JAVA_HOME is not set. Set it to your JDK 17 path, e.g.:
    echo   set JAVA_HOME=C:\Program Files\Eclipse Adoptium\jdk-17
    exit /b 1
)

set "KEYTOOL=%JAVA_HOME%\bin\keytool.exe"
set "CACERTS=%JAVA_HOME%\lib\security\cacerts"

if not exist "%KEYTOOL%" (
    echo Error: keytool not found at %KEYTOOL%
    echo Check that JAVA_HOME points to a valid JDK.
    exit /b 1
)

if not exist "%CACERTS%" (
    echo Error: cacerts not found at %CACERTS%
    exit /b 1
)

echo Importing certificate into Java truststore...
echo JAVA_HOME=%JAVA_HOME%
echo Certificate=%CER_FILE%
echo.

rem Delete existing alias if present (ignore error)
"%KEYTOOL%" -delete -alias gradle -keystore "%CACERTS%" -storepass changeit 2>nul

"%KEYTOOL%" -importcert -alias gradle -file "%CER_FILE%" -keystore "%CACERTS%" -storepass changeit
if %ERRORLEVEL% neq 0 (
    echo.
    echo Import failed. Try:
    echo 1. Run this script as Administrator.
    echo 2. If store password is not "changeit", run keytool manually with the correct -storepass.
    echo 3. Ensure the certificate file is valid PEM or DER .cer.
    exit /b 1
)

echo.
echo Certificate imported. Restart the terminal and run: gradlew.bat buildPlugin
endlocal
