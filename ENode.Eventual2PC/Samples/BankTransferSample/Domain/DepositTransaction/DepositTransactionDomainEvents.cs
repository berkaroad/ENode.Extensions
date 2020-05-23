namespace BankTransferSample.Domain
{
    /// <summary>存款交易已开始
    /// </summary>
    public class DepositTransactionStartedEvent : ENode.Eventual2PC.Events.TransactionInitiatorTransactionStartedBase<DepositTransaction, string>
    {
        public string AccountId { get; private set; }
        public double Amount { get; private set; }

        public override string TransactionId => AggregateRootId;

        public DepositTransactionStartedEvent() { }
        public DepositTransactionStartedEvent(string accountId, double amount)
            : base((byte)TransactionTypes.DepositTransaction)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }

    /// <summary>
    /// 存款交易预存款已确认
    /// </summary>
    public class DepositTransactionPreCommitSucceedParticipantAdded : ENode.Eventual2PC.Events.TransactionInitiatorPreCommitSucceedParticipantAddedBase<DepositTransaction, string>
    {
        public DepositTransactionPreCommitSucceedParticipantAdded() { }

        public DepositTransactionPreCommitSucceedParticipantAdded(string accountId, string transactionId)
            :base(transactionId, (byte)TransactionTypes.DepositTransaction, new Eventual2PC.TransactionParticipantInfo(accountId, (byte)AggregateRootTypes.BankAccount))
        { }
    }


    /// <summary>存款交易预存款已确认（转账事务，仅有一个参与者，看起来这个事件显得多余）
    /// </summary>
    public class DepositTransactionAllParticipantPreCommitSucceedEvent : ENode.Eventual2PC.Events.TransactionInitiatorAllParticipantPreCommitSucceedBase<DepositTransaction, string>
    {
        public DepositTransactionAllParticipantPreCommitSucceedEvent() { }
        public DepositTransactionAllParticipantPreCommitSucceedEvent(string accountId, string transactionId)
             : base(transactionId, (byte)TransactionTypes.DepositTransaction, new Eventual2PC.TransactionParticipantInfo[] { new Eventual2PC.TransactionParticipantInfo(accountId, (byte)AggregateRootTypes.BankAccount) })
        { }
    }


    /// <summary>存款交易存款金额已添加
    /// </summary>
    public class DepositTransactionCommittedParticipantAddedEvent : ENode.Eventual2PC.Events.TransactionInitiatorCommittedParticipantAddedBase<DepositTransaction, string>
    {
        public DepositTransactionCommittedParticipantAddedEvent() { }
        public DepositTransactionCommittedParticipantAddedEvent(string accountId, string transactionId)
            : base(transactionId, (byte)TransactionTypes.DepositTransaction, new Eventual2PC.TransactionParticipantInfo(accountId, (byte)AggregateRootTypes.BankAccount))
        { }
    }


    /// <summary>存款交易已完成
    /// </summary>
    public class DepositTransactionCompletedEvent : ENode.Eventual2PC.Events.TransactionInitiatorTransactionCompletedBase<DepositTransaction, string>
    {
        public DepositTransactionCompletedEvent() { }
        public DepositTransactionCompletedEvent(string transactionId, bool isCommitSuccess)
            :base(transactionId, (byte)TransactionTypes.DepositTransaction, isCommitSuccess)
        {
        }
    }
}
