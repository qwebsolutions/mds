#!/bin/sh

source ./projects.sh

for repo in ${repos[@]}
do
	repoName=basename $repo
	echo $(git -C $repo rev-parse HEAD) > ../${repoName}_commit.txt
done

outputFolder=../nugets

rm ../nugets -rf
mkdir ../nugets

rm ../global -rf
mkdir ../global

echo "Output folder: $outputFolder"

for p in ${projects[@]}
do
	dotnet pack $p -o $outputFolder -c Debug -p:Version="0.0.0-dev" -p:MetapsiVersion="0.0.0-dev" -p:MetapsiServiceVersion="0.0.0-dev" -p:RestoreAdditionalProjectSources=$(pwd)/../nugets -p:RestorePackagesPath=$(pwd)/../global
done

dotnet clean ../

for p in ${projects[@]}
do
	name=$(basename $p)
	lowercasenuget=$(echo "$name" | tr '[:upper:]' '[:lower:]')
	nugetpath=~/.nuget/packages/$lowercasenuget/0.0.0-dev
	rm -r -f $nugetpath
	echo removed $nugetpath
done

export NUGET_PACKAGES=$(pwd)/../global
echo Using global $NUGET_PACKAGES

dotnet restore ../ -p:MetapsiVersion="0.0.0-dev" -p:MetapsiServiceVersion="0.0.0-dev" -p:RestoreAdditionalProjectSources=$(pwd)/../nugets --no-cache --force -p:TreatWarningsAsErrors

#rm -rf $(pwd)/../global
