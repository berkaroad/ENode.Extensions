using System;
using ENode.Commanding;
using Eventual2PC.Commands;

namespace ENode.Eventual2PC.Commands
{
    /// <summary>
    /// 事务发起方添加已回滚的参与方
    /// </summary>
    [Serializable]
    public abstract class TransactionInitiatorAddRolledbackParticipantCommandBase<TAggregateRootId>
        : Command<TAggregateRootId>, ITransactionInitiatorAddRolledbackParticipantCommand
    {
        /// <summary>
        /// 事务发起方添加已回滚的参与方
        /// </summary>
        public TransactionInitiatorAddRolledbackParticipantCommandBase()
            : base()
        { }

        /// <summary>
        /// 事务发起方添加已回滚的参与方
        /// </summary>
        /// <param name="aggregateRootId">聚合根ID</param>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantId">事务参与方ID</param>
        /// <param name="participantType">事务参与方类型</param>
        public TransactionInitiatorAddRolledbackParticipantCommandBase(TAggregateRootId aggregateRootId, string transactionId, byte transactionType, string participantId, byte participantType)
            : base(aggregateRootId)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            ParticipantId = participantId;
            ParticipantType = participantType;
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
        /// 事务参与方ID
        /// </summary>
        public string ParticipantId { get; set; }

        /// <summary>
        /// 事务参与方类型
        /// </summary>
        public byte ParticipantType { get; set; }
    }
}