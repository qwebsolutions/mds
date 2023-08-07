set publishFolder=..\MdsLocal\bin\Release\net7.0\win10-x64\publish
set versionFile=_version.txt

rmdir ..\MdsLocal\obj /s /q
rmdir ..\MdsLocal\bin /s /q
del .\MdsLocalWindows.zip
dotnet publish ..\MdsLocal -c Release -r win10-x64 --self-contained
git status -s >> %publishFolder%\%versionFile%
git rev-parse --short HEAD >> %publishFolder%\%versionFile%
7z a .\MdsLocalWindows.zip %publishFolder%\*