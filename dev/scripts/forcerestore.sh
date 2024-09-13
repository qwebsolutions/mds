#!/bin/sh

dotnet clean ../Mds.Dev.sln

source ./projects.sh
for p in ${projects[@]}
do
	lowercasenuget=$(echo "$p" | tr '[:upper:]' '[:lower:]')
	nugetpath=~/.nuget/packages/$lowercasenuget/0.0.0-dev
	rm -r -f $nugetpath
	echo removed $nugetpath
done

for p in ${services[@]}
do
	lowercasenuget=$(echo "$p" | tr '[:upper:]' '[:lower:]')
	nugetpath=~/.nuget/packages/$lowercasenuget/0.0.0-dev
	rm -r -f $nugetpath
	echo removed $nugetpath
done

dotnet restore ../Mds.Dev.sln -p:MetapsiVersion="0.0.0-dev" -p:RestoreAdditionalProjectSources=$(pwd)/../nugets --no-cache --force

rm -rf $(pwd)/../global
