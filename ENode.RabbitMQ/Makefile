all: pack

test:
	dotnet run --project `pwd`/Samples/BankTransferSample

test-equeue:
	dotnet run --project `pwd`/Samples/BankTransferSample2

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/ENode.RabbitMQ/
	mv `pwd`/ENode.RabbitMQ/bin/Release/*.nupkg `pwd`/packages/

build:
	dotnet build -c Release `pwd`/ENode.RabbitMQ/
