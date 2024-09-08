#!/bin/sh

frameworkRoot=c:/github/qwebsolutions/metapsi
servicesRoot=c:/github/qwebsolutions/metapsi.services
outputFolder=../nugets

echo $(git -C $frameworkRoot rev-parse HEAD) > ../framework_commit.txt
echo $(git -C $servicesRoot rev-parse HEAD) > ../services_commit.txt

rm ../nugets -rf

echo "Output folder: $outputFolder"

source ./projects.sh
for p in ${projects[@]}
do
	dotnet pack $frameworkRoot/$p -o $outputFolder -c Debug -p:Version="0.0.0-dev" -p:MetapsiVersion="0.0.0-dev" -p:RestoreAdditionalProjectSources=$(pwd)/../nugets
done


for p in ${services[@]}
do
	dotnet pack $servicesRoot/$p -o $outputFolder -c Debug -p:Version="0.0.0-dev" -p:MetapsiVersion="0.0.0-dev" -p:RestoreAdditionalProjectSources=$(pwd)/../nugets
done

