set publishFolder=..\MdsLocal\bin\Release\net7.0\linux-x64\publish
set versionFile=_version.txt

rmdir ..\MdsLocal\obj /s /q
rmdir ..\MdsLocal\bin /s /q
del .\MdsLocalLinux.zip
dotnet publish ..\MdsLocal -c Release -r linux-x64 --self-contained
git status -s >> %publishFolder%\%versionFile%
git rev-parse --short HEAD >> %publishFolder%\%versionFile%
7z a .\MdsLocalLinux.zip %publishFolder%\*