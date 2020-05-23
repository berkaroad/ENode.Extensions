using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>
    /// 发起一笔转账交易
    /// </summary>
    public class StartTransferTransactionCommand : Command
    {
        /// <summary>
        /// 转账交易信息
        /// </summary>
        public TransferTransactionInfo TransactionInfo { get; set; }

        public StartTransferTransactionCommand() { }
        public StartTransferTransactionCommand(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }

    /// <summary>
    /// 添加转账事务预提交失败的参与者
    /// </summary>
    public class AddTransferPreCommitSuccessParticipantCommand
        : ENode.Eventual2PC.Commands.TransactionInitiatorAddPreCommitSucceedParticipantCommandBase<string>
    {
        
    }

    /// <summary>
    /// 添加转账事务预提交失败的参与者
    /// </summary>
    public class AddTransferPreCommitFailParticipantCommand
        : ENode.Eventual2PC.Commands.TransactionInitiatorAddPreCommitFailedParticipantCommandBase<string>
    {
        
    }

    /// <summary>
    /// 添加转账事务已提交的参与者
    /// </summary>
    public class AddTransferCommittedParticipantCommand
        : ENode.Eventual2PC.Commands.TransactionInitiatorAddCommittedParticipantCommandBase<string>
    {     
    }

    /// <summary>
    /// 添加转账事务已回滚的参与者
    /// </summary>
    public class AddTransferRolledbackParticipantCommand
        : ENode.Eventual2PC.Commands.TransactionInitiatorAddRolledbackParticipantCommandBase<string>
    {
    }
}
