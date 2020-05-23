using ENode.Eventing;
using System.Collections.Generic;

namespace BankTransferSample.Domain
{
    /// <summary>
    /// 转账交易事件基类
    /// </summary>
    public abstract class AbstractTransferTransactionEvent : DomainEvent<string>
    {
        public TransferTransactionInfo TransactionInfo { get; private set; }

        public AbstractTransferTransactionEvent() { }
        public AbstractTransferTransactionEvent(TransferTransactionInfo transactionInfo)
        {
            TransactionInfo = transactionInfo;
        }
    }

    /// <summary>
    /// 转账交易已开始
    /// </summary>
    public class TransferTransactionStartedEvent : ENode.Eventual2PC.Events.TransactionInitiatorTransactionStartedBase<TransferTransaction, string>
    {
        public override string TransactionId => AggregateRootId;
        public TransferTransactionInfo TransactionInfo { get; private set; }

        public TransferTransactionStartedEvent() { }
        public TransferTransactionStartedEvent(TransferTransactionInfo transactionInfo)
            : base((byte)TransactionTypes.TransferTransaction)
        {
            TransactionInfo = transactionInfo;
        }
    }

    /// <summary>
    /// 转账交易预提交成功的银行账户已添加
    /// </summary>
    public class TransferTransactionPreCommitSucceedParticipantAdded : ENode.Eventual2PC.Events.TransactionInitiatorPreCommitSucceedParticipantAddedBase<TransferTransaction, string>
    {
        public PreparationTypes PreparationType { get; set; }
        public TransferTransactionPreCommitSucceedParticipantAdded() { }

        public TransferTransactionPreCommitSucceedParticipantAdded(string accountId, string transactionId, PreparationTypes preparationType)
            : base(transactionId, (byte)TransactionTypes.TransferTransaction, new Eventual2PC.TransactionParticipantInfo(accountId, (byte)AggregateRootTypes.BankAccount))
        {
            PreparationType = preparationType;
        }
    }

    /// <summary>
    /// 转账交易预提交失败的银行账户已添加
    /// </summary>
    public class TransferTransactionPreCommitFailedParticipantAdded : ENode.Eventual2PC.Events.TransactionInitiatorPreCommitFailedParticipantAddedBase<TransferTransaction, string>
    {
        public TransferTransactionPreCommitFailedParticipantAdded() { }

        public TransferTransactionPreCommitFailedParticipantAdded(string accountId, string transactionId)
            : base(transactionId, (byte)TransactionTypes.TransferTransaction, new Eventual2PC.TransactionParticipantInfo(accountId, (byte)AggregateRootTypes.BankAccount))
        { }
    }

    /// <summary>
    /// 转账交易所有银行账户都预提交成功
    /// </summary>
    public class TransferTransactionAllParticipantPreCommitSucceedEvent : ENode.Eventual2PC.Events.TransactionInitiatorAllParticipantPreCommitSucceedBase<TransferTransaction, string>
    {
        public TransferTransactionAllParticipantPreCommitSucceedEvent() { }
        public TransferTransactionAllParticipantPreCommitSucceedEvent(string transactionId, IEnumerable<Eventual2PC.TransactionParticipantInfo> participantInfos)
             : base(transactionId, (byte)TransactionTypes.TransferTransaction, participantInfos)
        {
        }
    }

    /// <summary>
    /// 转账交易存在某个银行账户预提交失败
    /// </summary>
    public class TransferTransactionAnyParticipantPreCommitFailedEvent : ENode.Eventual2PC.Events.TransactionInitiatorAnyParticipantPreCommitFailedBase<TransferTransaction, string>
    {
        public TransferTransactionAnyParticipantPreCommitFailedEvent() { }
        public TransferTransactionAnyParticipantPreCommitFailedEvent(string transactionId, IEnumerable<Eventual2PC.TransactionParticipantInfo> participantInfos)
             : base(transactionId, (byte)TransactionTypes.TransferTransaction, participantInfos)
        { }
    }


    /// <summary>
    /// 转账交易，已提交的银行账号已添加
    /// </summary>
    public class TransferTransactionCommittedParticipantAddedEvent : ENode.Eventual2PC.Events.TransactionInitiatorCommittedParticipantAddedBase<TransferTransaction, string>
    {
        public TransferTransactionCommittedParticipantAddedEvent() { }
        public TransferTransactionCommittedParticipantAddedEvent(string accountId, string transactionId)
            : base(transactionId, (byte)TransactionTypes.TransferTransaction, new Eventual2PC.TransactionParticipantInfo(accountId, (byte)AggregateRootTypes.BankAccount))
        { }
    }

    /// <summary>
    /// 转账交易，已回滚的银行账号已添加
    /// </summary>
    public class TransferTransactionRolledbackParticipantAddedEvent : ENode.Eventual2PC.Events.TransactionInitiatorRolledbackParticipantAddedBase<TransferTransaction, string>
    {
        public TransferTransactionRolledbackParticipantAddedEvent() { }
        public TransferTransactionRolledbackParticipantAddedEvent(string accountId, string transactionId)
            : base(transactionId, (byte)TransactionTypes.TransferTransaction, new Eventual2PC.TransactionParticipantInfo(accountId, (byte)AggregateRootTypes.BankAccount))
        { }
    }


    /// <summary>
    /// 转账交易已完成
    /// </summary>
    public class TransferTransactionCompletedEvent : ENode.Eventual2PC.Events.TransactionInitiatorTransactionCompletedBase<TransferTransaction,string>
    {
        public TransferTransactionCompletedEvent() { }
        public TransferTransactionCompletedEvent(string transactionId, bool isCommitSuccess)
            : base(transactionId, (byte)TransactionTypes.TransferTransaction, isCommitSuccess)
        {
        }
    }
}
