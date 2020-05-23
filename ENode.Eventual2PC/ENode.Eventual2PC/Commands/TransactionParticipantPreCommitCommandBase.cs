using System;
using ENode.Commanding;
using Eventual2PC.Commands;

namespace ENode.Eventual2PC.Commands
{
    /// <summary>
    /// 事务参与方预提交命令
    /// </summary>
    [Serializable]
    public abstract class TransactionParticipantPreCommitCommandBase<TAggregateRootId>
        : Command<TAggregateRootId>, ITransactionParticipantPreCommitCommand
    {
        /// <summary>
        /// 事务参与方预提交命令
        /// </summary>
        public TransactionParticipantPreCommitCommandBase()
            : base()
        { }

        /// <summary>
        /// 事务参与方预提交命令
        /// </summary>
        /// <param name="aggregateRootId">聚合根ID</param>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="initiatorId">事务发起方ID</param>
        /// <param name="initiatorType">事务发起方类型</param>
        public TransactionParticipantPreCommitCommandBase(TAggregateRootId aggregateRootId, string transactionId, byte transactionType, string initiatorId, byte initiatorType)
            : base(aggregateRootId)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            InitiatorId = initiatorId;
            InitiatorType = initiatorType;
        }

        /// <summary>
        /// 事务ID
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// 事务类型
        /// </summary>
        public byte TransactionType { get; set; }
        
        /// <summary>
        /// 事务发起方ID
        /// </summary>
        public string InitiatorId { get; set; }
        
        /// <summary>
        /// 事务发起方类型
        /// </summary>
        public byte InitiatorType { get; set; }
    }
}