@echo off
setlocal

pushd "%~dp0"
electronize build /target win /package-json electron.package.json
set EXIT_CODE=%ERRORLEVEL%
popd

exit /b %EXIT_CODE%
