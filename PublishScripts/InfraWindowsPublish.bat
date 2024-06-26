set publishFolder=..\MdsInfrastructure\bin\Release\net7.0\win10-x64\publish
set versionFile=_version.txt

rmdir ..\MdsInfrastructure\obj /s /q
rmdir ..\MdsInfrastructure\bin /s /q
del .\MdsInfrastructureWindows.zip
dotnet publish ..\MdsInfrastructure -c Release -r win10-x64 --self-contained
git status -s >> %publishFolder%\%versionFile%
git rev-parse --short HEAD >> %publishFolder%\%versionFile%
7z a .\MdsInfrastructureWindows.zip %publishFolder%\*