using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>
    /// 开户（创建一个账户）
    /// </summary>
    public class CreateAccountCommand : Command
    {
        public string Owner { get; set; }

        public CreateAccountCommand() { }
        public CreateAccountCommand(string accountId, string owner) : base(accountId)
        {
            Owner = owner;
        }
    }

    /// <summary>
    /// 验证账户是否合法
    /// </summary>
    public class ValidateAccountCommand : Command
    {
        public string TransactionId { get; set; }
        public byte TransactionType { get; set; }

        public PreparationTypes PreparationType { get; set; }

        public double Amount { get; set; }

        public ValidateAccountCommand() { }
        public ValidateAccountCommand(string accountId, string transactionId, byte transactionType, PreparationTypes preparationType, double amount) : base(accountId)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            PreparationType = preparationType;
            Amount = amount;
        }
    }

    /// <summary>
    /// 向账户添加一笔预操作
    /// </summary>
    public class PreCommitDepositTransactionPreparationCommand
        : ENode.Eventual2PC.Commands.TransactionParticipantPreCommitCommandBase<string>
    {
        public double Amount { get; set; }
    }

    /// <summary>
    /// 向账户添加一笔预操作
    /// </summary>
    public class PreCommitWithdrawTransactionPreparationCommand
        : ENode.Eventual2PC.Commands.TransactionParticipantPreCommitCommandBase<string>
    {
        public double Amount { get; set; }
    }

    /// <summary>
    /// 提交预操作
    /// </summary>
    public class CommitTransactionPreparationCommand
        : ENode.Eventual2PC.Commands.TransactionParticipantCommitCommandBase<string>
    {
        public CommitTransactionPreparationCommand() { }
        public CommitTransactionPreparationCommand(string accountId, string transactionId)
            : base(accountId, transactionId)
        {
        }
    }

    /// <summary>
    /// 回滚预操作
    /// </summary>
    public class RollbackTransactionPreparationCommand
        : ENode.Eventual2PC.Commands.TransactionParticipantRollbackCommandBase<string>
    {
        public RollbackTransactionPreparationCommand() { }
        public RollbackTransactionPreparationCommand(string accountId, string transactionId)
            : base(accountId, transactionId)
        {
        }
    }
}
