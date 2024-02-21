@echo off

if not exist infra_url.txt (
	ECHO ERROR: File infra_url.txt should contain the API base path
	goto :EOF
)

set /p baseUrl=<infra_url.txt
curl -H "Content-Type: application/json" -d @credentials.json --cookie-jar cookies.txt --fail-with-body --silent -S %baseUrl%/api/signin 
IF %ERRORLEVEL% NEQ 0 (
  ECHO ERROR: Unable to login to %baseUrl%
  goto :cleanup
)
del configuration.current.json
curl --cookie cookies.txt -o configuration.current.json --fail-with-body --silent %baseUrl%/api/configuration
IF %ERRORLEVEL% NEQ 0 (
  ECHO ERROR: Unable to retrieve configuration.current.json from %baseUrl%/api/configuration
  goto :cleanup
)

if not exist configuration.next.json (
	ECHO File configuration.next.json not found. Initializing with current configuration.
	copy configuration.current.json configuration.next.json
)

git --no-pager diff --no-index configuration.current.json configuration.next.json

curl -H "Content-Type: application/json" -d @configuration.next.json --cookie cookies.txt --fail-with-body --silent %baseUrl%/api/checkconfiguration
IF %ERRORLEVEL% NEQ 0 (
  ECHO ERROR: Unable to post configuration.next.json to %baseUrl%/api/checkconfiguration
  goto :EOF
)

:cleanup
del cookies.txt