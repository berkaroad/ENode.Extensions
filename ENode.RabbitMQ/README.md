# ENode.RabbitMQ
RabbitMQ adapter for ENode.

Reference: [RabbitMQTopic](https://github.com/berkaroad/RabbitMQTopic)

## Install

```
dotnet add package ENode.RabbitMQ
```

## Usage

See [BankTransferSample](Samples/BankTransferSample/ENodeExtensions.cs)

## Performance

Test at 8Core Macbook, that running both RabbitMQ and test program.

```
All transfer transactions completed, time spent: 6226ms, throughput: 160 transactions per second.
Concurrent test complete.
```

## Publish History

### 1.1.4
1）Upgrade RabbitMQTopic package

### 1.1.3
1）Upgrade RabbitMQTopic package

### 1.1.2
1）Fix CommandConsumer when replyaddress is empty

### 1.1.1
1）`CommandService`'s method 'InitializeRabbitMQ' add parameter 'delayedCommandEnabled' and default is false, because not all rabbitmq enable plugin 'rabbitmq_delayed_message_exchange'.

### 1.1.0
1）Delayed Command supported. By using `ICommandService`'s extension method `SendDelayedCommandAsync` or `ExecuteDelayedCommandAsync`

2）Adjust method 'InitializeRabbitMQ' of publisher and consumer

3）Topic's queue count support user-defined, default is 4.

### 1.0.6
1）Command Message add property "SagaId", for saga when sending command with item key "SagaId",  and that you could change it.

2）The command message will not ack when "SagaId" exists and `commandHandler` handle failed, except result type end with "DomainException".

3）Fix the bug that UI call "SendAsync" of "CommandService" will not ack when command is executed fail.

### 1.0.5
1）The `command` message will not ack when `commandHandler` handle failed, except result type that end with "DomainException".

### 1.0.4
1）If the `commandResultProcessor` not set in `CommandService`, the command message will not ack when `commandHandler` handle failed.

### 1.0.3

1）Upgrade depended nuget package RabbitMQTopic 1.2.1.

2）Add error log when consume message failed.

### 1.0.2
1）RabbitMQ adapter for ENode.
