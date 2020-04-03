#!/bin/sh

set -e

dotnet clean
dotnet run -p tools Yandex.Cloud.SDK/Generated.cs
dotnet test
