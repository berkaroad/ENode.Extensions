using System;
using ENode.Commanding;
using Eventual2PC.Commands;

namespace ENode.Eventual2PC.Commands
{
    /// <summary>
    /// 事务参与方提交命令
    /// </summary>
    [Serializable]
    public abstract class TransactionParticipantCommitCommandBase<TAggregateRootId>
        : Command<TAggregateRootId>, ITransactionParticipantCommitCommand
    {
        /// <summary>
        /// 事务参与方提交命令
        /// </summary>
        public TransactionParticipantCommitCommandBase()
            : base()
        { }

        /// <summary>
        /// 事务参与方提交命令
        /// </summary>
        /// <param name="aggregateRootId">聚合根ID</param>
        /// <param name="transactionId">事务ID</param>
        public TransactionParticipantCommitCommandBase(TAggregateRootId aggregateRootId, string transactionId)
            : base(aggregateRootId)
        {
            TransactionId = transactionId;
        }

        /// <summary>
        /// 事务ID
        /// </summary>
        public string TransactionId { get; set; }
    }
}