@echo off

if not exist infra_url.txt (
	ECHO ERROR: File infra_url.txt should contain the API base path
	goto :EOF
)

set /p baseUrl=<infra_url.txt
set /p configurationId=<configuration_id.txt
curl -H "Content-Type: application/json" -d @credentials.json --cookie-jar cookies.txt --fail-with-body --silent %baseUrl%/api/signin 
IF %ERRORLEVEL% NEQ 0 (
  ECHO ERROR Unable to login to %baseUrl%
  goto :EOF
)

git rev-parse --is-inside-work-tree 2>nul >usegit.txt
set /p usegit=<usegit.txt
del usegit.txt
if "%useGit%"=="true" (
	git pull
)

curl -H "Content-Type: application/json" -d @configuration.next.json --cookie cookies.txt --fail-with-body --silent %baseUrl%/api/configuration
IF %ERRORLEVEL% NEQ 0 (
  ECHO ECHO ERROR: Unable to post configuration.next.json to %baseUrl%/api/configuration
  goto :cleanup
)

curl --cookie cookies.txt -o configuration.current.json --fail-with-body --silent %baseUrl%/api/configuration/%configurationId%
IF %ERRORLEVEL% NEQ 0 (
  ECHO ECHO ERROR: Unable to retrieve configuration.current.json from %baseUrl%/api/configuration
  goto :cleanup
)
curl --cookie cookies.txt -o configuration.next.json --fail-with-body --silent %baseUrl%/api/configuration/%configurationId%
IF %ERRORLEVEL% NEQ 0 (
  ECHO ECHO ERROR: Unable to retrieve configuration.next.json from %baseUrl%/api/configuration
  goto :cleanup
)

if "%useGit%"=="true" (
	git add .
	git commit -m "upload.bat"
	git push
)

:cleanup
del cookies.txt