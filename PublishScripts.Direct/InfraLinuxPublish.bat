set publishFolder=..\MdsInfrastructure\bin\Release\net7.0\linux-x64\publish
set versionFile=_version.txt

rmdir ..\MdsInfrastructure\obj /s /q
rmdir ..\MdsInfrastructure\bin /s /q
del .\MdsInfrastructureLinux.zip
dotnet publish ..\MdsInfrastructure\MdsInfrastructure.Direct.csproj -c Release -r linux-x64 --self-contained
git status -s >> %publishFolder%\%versionFile%
git rev-parse --short HEAD >> %publishFolder%\%versionFile%
7z a .\MdsInfrastructureLinux.zip %publishFolder%\*