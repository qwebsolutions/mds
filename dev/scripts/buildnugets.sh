#!/bin/sh

frameworkRoot=c:/github/qwebsolutions/metapsi
outputFolder=../nugets

echo $(git -C $frameworkRoot rev-parse HEAD) > ../framework_commit.txt

rm ../nugets -rf

echo "Output folder: $outputFolder"

source ./projects.sh
for p in ${projects[@]}
do
	dotnet pack $frameworkRoot/$p -o $outputFolder -c Debug -p:Version="0.0.0-dev" -p:MetapsiVersion="0.0.0-dev" -p:RestoreAdditionalProjectSources=$(pwd)/../nugets
done


