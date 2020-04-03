clean:
	dotnet clean
	rm -rf dist
	find . -name '*.nupkg' | xargs -L1 -I{} rm {}

build:
	dotnet build tools/tools.csproj
	dotnet build

test:
	dotnet test

package:
	dotnet pack
	mkdir -p dist
	find . -name '*.nupkg' | xargs -L1 -I{} cp {} dist/

all: clean build test package
