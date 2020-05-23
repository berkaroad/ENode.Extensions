all: pack

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/ENode.Eventual2PC/
	mv `pwd`/ENode.Eventual2PC/bin/Release/*.nupkg `pwd`/packages/

test:
	dotnet run --project `pwd`/Samples/BankTransferSample

build:
	dotnet build -c Release `pwd`/ENode.Eventual2PC/
