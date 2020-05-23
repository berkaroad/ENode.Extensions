using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>
    /// 发起一笔存款交易
    /// </summary>
    public class StartDepositTransactionCommand : Command
    {
        /// <summary>
        /// 账户ID
        /// </summary>
        public string AccountId { get; set; }
        
        /// <summary>
        /// 存款金额
        /// </summary>
        public double Amount { get; set; }

        public StartDepositTransactionCommand() { }
        public StartDepositTransactionCommand(string transactionId, string accountId, double amount)
            : base(transactionId)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }

    /// <summary>
    /// 添加预提交失败的参与者
    /// </summary>
    public class AddDepositPreCommitSuccessParticipantCommand
        : ENode.Eventual2PC.Commands.TransactionInitiatorAddPreCommitSucceedParticipantCommandBase<string>
    {
        
    }

    /// <summary>
    /// 添加已提交的参与者
    /// </summary>
    public class AddDepositCommittedParticipantCommand
        : ENode.Eventual2PC.Commands.TransactionInitiatorAddCommittedParticipantCommandBase<string>
    {
    }
}
