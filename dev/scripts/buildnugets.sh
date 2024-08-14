#!/bin/sh

frameworkRoot=c:/github/qwebsolutions/metapsi
outputFolder=../nugets

REPO_COMMIT=$(git rev-parse HEAD)
echo $REPO_COMMIT > ../metapsi_version.txt

rm ../nugets -rf

echo "Output folder: $outputFolder"

source ./projects.sh
for p in ${projects[@]}
do
	dotnet pack $frameworkRoot/$p -o $outputFolder -c Debug -p:Version="0.0.0-dev"
done


