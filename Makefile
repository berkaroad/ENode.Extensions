all: pack

test:
	dotnet test `pwd`/ENode.PublishedVersionStore.Redis/ENode.PublishedVersionStore.Redis.Tests

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/ENode.PublishedVersionStore.Redis/ENode.PublishedVersionStore.Redis/
	mv `pwd`/ENode.PublishedVersionStore.Redis/ENode.PublishedVersionStore.Redis/bin/Release/*.nupkg `pwd`/packages/

build:
	dotnet build -c Release `pwd`/ENode.PublishedVersionStore.Redis/ENode.PublishedVersionStore.Redis/
