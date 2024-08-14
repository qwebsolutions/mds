#!/bin/sh

frameworkRoot=c:/github/qwebsolutions/metapsi
outputFolder=../nugets

REPO_COMMIT=$(git rev-parse HEAD)
echo $REPO_COMMIT > ../metapsi_version.txt

rm ../nugets -rf

echo "Output folder: $outputFolder"

projects=("Metapsi.Runtime" "Metapsi.Hyperapp" "Metapsi.Mds" "Metapsi.Redis" "Metapsi.Ui" "Metapsi.Web.Contracts" "Metapsi.Web" "Metapsi.Html" "Metapsi.Shoelace" "Metapsi.SQLite" "Metapsi.TomSelect" "Metapsi.Dom" "Metapsi.Module" "Metapsi.Timer" "Metapsi.JavaScript" "Metapsi.Heroicons")
for p in ${projects[@]}
do
	dotnet pack $frameworkRoot/$p -o $outputFolder -c Debug -p:Version="0.0.0-dev"
done


