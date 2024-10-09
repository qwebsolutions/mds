#!/bin/sh

repos=($metapsi $services)

metapsi=c:/github/qwebsolutions/metapsi
services=c:/github/qwebsolutions/metapsi.services

projects=( \
$metapsi/Metapsi.Runtime \
$metapsi/Metapsi.Mds \
$metapsi/Metapsi.Redis \
$metapsi/Metapsi.Web \
$metapsi/Metapsi.Web.Contracts \
$metapsi/Metapsi.Module \
$metapsi/Metapsi.JavaScript \
$metapsi/Metapsi.Html \
$metapsi/Metapsi.Heroicons \
$metapsi/Metapsi.Shoelace \
$metapsi/Metapsi.SQLite \
$metapsi/Metapsi.TomSelect \
$metapsi/Metapsi.Timer \
$services/Metapsi.ServiceDoc)

echo ${projects[*]}

#export NUGET_PACKAGES=$(pwd)/../global
#echo Using global $NUGET_PACKAGES

# if you want to build a specific project comment the list
#projects=("Metapsi.Runtime")

#projects=("Metapsi.Runtime" "Metapsi.Mds" "Metapsi.Redis" "Metapsi.Web" "Metapsi.Web.Contracts" "Metapsi.Module" "Metapsi.JavaScript" "Metapsi.Html" "Metapsi.Shoelace" "Metapsi.SQLite" "Metapsi.TomSelect" "Metapsi.Timer" "Metapsi.Heroicons")
#services=("Metapsi.ServiceDoc")
