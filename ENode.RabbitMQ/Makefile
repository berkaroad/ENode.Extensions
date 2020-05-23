all: pack

test:
	dotnet run --project `pwd`/Samples/BankTransferSample

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/ENode.RabbitMQ/
	mv `pwd`/ENode.RabbitMQ/bin/Release/*.nupkg `pwd`/packages/

build:
	dotnet build -c Release `pwd`/ENode.RabbitMQ/
