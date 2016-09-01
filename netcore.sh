#!/usr/bin/env bash

#exit if any command fails
set -e

debug=false

if [ "$debug" = false ] ; then
  configuration="-c Release"
else
  configuration="-c Debug"
fi  

if [ "$debug" = true ] ; then
  libLoc="bin/Debug"
else
  libLoc="bin/Release"
fi  

function Build {
  dotnet restore

  dotnet build ./src/NativeLibraryUtilities $configuration
}

Build

