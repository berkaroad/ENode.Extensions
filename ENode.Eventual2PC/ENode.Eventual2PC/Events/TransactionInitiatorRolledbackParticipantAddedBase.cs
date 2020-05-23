using System;
using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 已回滚的事务参与方已添加事件基类
    /// </summary>
    /// <typeparam name="TInitiator">事务发起方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorRolledbackParticipantAddedBase<TInitiator, TAggregateRootId>
        : DomainEvent<TAggregateRootId>
        , ITransactionInitiatorRolledbackParticipantAdded<TInitiator>
        where TInitiator : AggregateRoot<TAggregateRootId>, ITransactionInitiator
    {
        /// <summary>
        /// 已回滚的事务参与方已添加事件基类
        /// </summary>
        protected TransactionInitiatorRolledbackParticipantAddedBase() { }

        /// <summary>
        /// 已回滚的事务参与方已添加事件基类
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected TransactionInitiatorRolledbackParticipantAddedBase(string transactionId, byte transactionType, TransactionParticipantInfo transactionParticipant)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            TransactionParticipant = transactionParticipant;
        }

        /// <summary>
        /// 事务ID
        /// </summary>
        public string TransactionId { get; private set; }

        /// <summary>
        /// 事务类型
        /// </summary>
        public byte TransactionType { get; private set; }

        /// <summary>
        /// 事务参与方信息
        /// </summary>
        public TransactionParticipantInfo TransactionParticipant { get; private set; }
    }
}
