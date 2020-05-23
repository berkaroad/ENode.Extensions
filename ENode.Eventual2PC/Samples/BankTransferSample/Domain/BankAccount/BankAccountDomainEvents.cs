using ENode.Eventing;

namespace BankTransferSample.Domain
{
    /// <summary>
    /// 已开户
    /// </summary>
    public class AccountCreatedEvent : DomainEvent<string>
    {
        /// <summary>
        /// 账户拥有者
        /// </summary>
        public string Owner { get; private set; }

        public AccountCreatedEvent() { }
        public AccountCreatedEvent(string owner)
        {
            Owner = owner;
        }
    }

    /// <summary>
    /// 账户预转出操作已添加
    /// </summary>
    public class WithdrawTransactionPreCommitSucceedEvent : ENode.Eventual2PC.Events.TransactionParticipantPreCommitSucceedBase<BankAccount, string, WithdrawTransactionPreparation>
    {
        public WithdrawTransactionPreCommitSucceedEvent() { }
        public WithdrawTransactionPreCommitSucceedEvent(WithdrawTransactionPreparation transactionPreparation)
            : base(transactionPreparation)
        {
        }
    }

    /// <summary>
    /// 账户预转出操作已执行
    /// </summary>
    public class WithdrawTransactionCommittedEvent : ENode.Eventual2PC.Events.TransactionParticipantCommittedBase<BankAccount, string, WithdrawTransactionPreparation>
    {
        public double CurrentBalance { get; private set; }

        public WithdrawTransactionCommittedEvent() { }
        public WithdrawTransactionCommittedEvent(double currentBalance, WithdrawTransactionPreparation transactionPreparation)
              : base(transactionPreparation)
        {
            CurrentBalance = currentBalance;
        }
    }

    /// <summary>
    /// 账户预转出操作已取消
    /// </summary>
    public class WithdrawTransactionRolledbackEvent : ENode.Eventual2PC.Events.TransactionParticipantRolledbackBase<BankAccount, string, WithdrawTransactionPreparation>
    {
        public WithdrawTransactionRolledbackEvent() { }
        public WithdrawTransactionRolledbackEvent(WithdrawTransactionPreparation transactionPreparation)
              : base(transactionPreparation)
        {
        }
    }

    /// <summary>
    /// 账户预转入操作已添加
    /// </summary>
    public class DepositTransactionPreCommitSucceedEvent : ENode.Eventual2PC.Events.TransactionParticipantPreCommitSucceedBase<BankAccount, string, DepositTransactionPreparation>
    {
        public DepositTransactionPreCommitSucceedEvent() { }
        public DepositTransactionPreCommitSucceedEvent(DepositTransactionPreparation transactionPreparation)
            : base(transactionPreparation)
        {
        }
    }

    /// <summary>
    /// 账户预转入操作已提交
    /// </summary>
    public class DepositTransactionCommittedEvent : ENode.Eventual2PC.Events.TransactionParticipantCommittedBase<BankAccount, string, DepositTransactionPreparation>
    {
        public double CurrentBalance { get; private set; }

        public DepositTransactionCommittedEvent() { }
        public DepositTransactionCommittedEvent(double currentBalance, DepositTransactionPreparation transactionPreparation)
              : base(transactionPreparation)
        {
            CurrentBalance = currentBalance;
        }
    }

    /// <summary>
    /// 账户预转入操作已取消
    /// </summary>
    public class DepositTransactionRolledbackEvent : ENode.Eventual2PC.Events.TransactionParticipantRolledbackBase<BankAccount, string, DepositTransactionPreparation>
    {
        public DepositTransactionRolledbackEvent() { }
        public DepositTransactionRolledbackEvent(DepositTransactionPreparation transactionPreparation)
              : base(transactionPreparation)
        {
        }
    }
}
