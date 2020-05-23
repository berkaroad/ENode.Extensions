all: pack

test:
	dotnet run --project `pwd`/ENode.Eventual2PC/Samples/BankTransferSample
	dotnet test `pwd`/ENode.PublishedVersionStore.Redis/ENode.PublishedVersionStore.Redis.Tests

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/ENode.Eventual2PC/ENode.Eventual2PC/
	mv `pwd`/ENode.Eventual2PC/ENode.Eventual2PC/bin/Release/*.nupkg `pwd`/packages/

	dotnet pack -c Release `pwd`/ENode.AggregateSnapshotStore/ENode.AggregateSnapshotStore/
	mv `pwd`/ENode.AggregateSnapshotStore/ENode.AggregateSnapshotStore/bin/Release/*.nupkg `pwd`/packages/
	dotnet pack -c Release `pwd`/ENode.PublishedVersionStore.Redis/ENode.PublishedVersionStore.Redis/
	mv `pwd`/ENode.PublishedVersionStore.Redis/ENode.PublishedVersionStore.Redis/bin/Release/*.nupkg `pwd`/packages/

build:
	dotnet build -c Release `pwd`/ENode.Eventual2PC/ENode.Eventual2PC/
	dotnet build -c Release `pwd`/ENode.AggregateSnapshotStore/ENode.AggregateSnapshotStore/
	dotnet build -c Release `pwd`/ENode.PublishedVersionStore.Redis/ENode.PublishedVersionStore.Redis/
