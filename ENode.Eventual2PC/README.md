# ENode.Eventual2PC
Implement Eventual2PC by enode.

Reference [Eventual2PC](https://github.com/berkaroad/Eventual2PC)

## Install

```
dotnet add package ENode.Eventual2PC
```

## Usage

```csharp
// 银行账号（转账事务的参与者）
public BandAccount : ENode.Eventual2PC.TransactionParticipantBase<string>
{
    // 抽象方法实现
}

// 转账事务（转账事务的发起者）
public TransferTransaction : ENode.Eventual2PC.TransactionInitiatorBase<TransferTransaction, string>
{
    // 抽象方法实现
}
```

## Publish History

## 1.2.2

- 1）抽象类标记为可序列号；

- 2）TransactionInitiatorAlsoActAsOtherParticipantBase 添加虚属性 PreventPreCommitOrNotIfTransactionProcessing

## 1.2.1

- 1）TransactionInitiatorAlsoActAsOtherParticipantBase 在PreCommit如果已经发起事务，将发布AlreadyStartTransactionWhenPreCommitDomainException 领域异常（原是ApplicationException，不合理，因为这个属于业务操作产生，非程序bug）

## 1.2.0

- 实现 Eventual2PC 1.1版本的Command接口

## 1.1.1

- 1）修复`Initiator` 在执行 `AddPreCommitFailedParticipant` 时，如果所有预提交已添加且都失败，将无法触发事件 `TransactionCompleted` 

- 2）将类 `TransactionInitiatorAlsoActAsOtherParticipantBase` 继承自 `TransactionInitiatorBase` 来简化代码

## 1.1.0

- 1）`TransactionInitiatorBase` 添加校验逻辑，以符合 `Eventual2PC` 中的规约描述

- 2）新增 `TransactionInitiatorAlsoActAsOtherParticipantBase`，以满足一个聚合根实例既是事务A的 `Initiator`， 又是事务B的 `Participant` 的场景

- 3）小重构，将事件接口替换为抽象类，目的是为了减少使用方的编码量

## 1.0.5

- 1）修复 `TransactionInitiatorBase`、 `TransactionParticipantBase`内部处理

## 1.0.4

- 1）修复 `TransactionParticipantBase`内部处理


## 1.0.3

- 1）修复 `TransactionInitiatorBase`


## 1.0.2

- 1）取消自定义接口 `ITransactionPreparation`

## 1.0.1

- 1）增加 `PreCommitFailed` 的领域异常接口 `ITransactionParticipantPreCommitDomainException`

## 1.0.0

- 初始版本