#!/bin/sh

export NUGET_PACKAGES=$(pwd)/../global
echo Using global $NUGET_PACKAGES

# if you want to build a specific project comment the list
projects=("Metapsi.Runtime")

projects=("Metapsi.Runtime" "Metapsi.Mds" "Metapsi.Redis" "Metapsi.Web" "Metapsi.Web.Contracts" "Metapsi.Module" "Metapsi.JavaScript" "Metapsi.Html" "Metapsi.Shoelace" "Metapsi.SQLite" "Metapsi.TomSelect" "Metapsi.Timer" "Metapsi.Heroicons")
services=("Metapsi.ServiceDoc")
