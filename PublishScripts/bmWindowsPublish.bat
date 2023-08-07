del ..\MdsBuildManager\bin /q
del .\MdsBuildManager.zip
dotnet restore ..\MdsBuildManager
dotnet publish ..\MdsBuildManager -c Release -r win10-x64
7z a .\MdsBuildManager.zip ..\MdsBuildManager\bin\Release\net7.0\win10-x64\publish\*