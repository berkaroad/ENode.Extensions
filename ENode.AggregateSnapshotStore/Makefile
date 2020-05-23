all: pack

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/ENode.AggregateSnapshotStore/
	mv `pwd`/ENode.AggregateSnapshotStore/bin/Release/*.nupkg `pwd`/packages/

build:
	dotnet build -c Release `pwd`/ENode.AggregateSnapshotStore/
