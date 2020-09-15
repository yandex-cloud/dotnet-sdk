#!/bin/sh

set -e

dotnet clean
rm -rf dist
find . -name '*.nupkg' | xargs -L1 -I{} rm {}
dotnet build tools/tools.csproj
dotnet build
dotnet test
