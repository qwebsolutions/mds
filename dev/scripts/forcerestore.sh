#!/bin/sh

source ./projects.sh
for p in ${projects[@]}
do
	lowercasenuget=$(echo "$p" | tr '[:upper:]' '[:lower:]')
	nugetpath=~/.nuget/packages/$lowercasenuget/0.0.0-dev
	rm -r -f $nugetpath
	echo removed $nugetpath
done

dotnet restore ../Mds.Dev.sln

