using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;
using System;
using System.Collections.Generic;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方所有预提交已成功事件基类
    /// </summary>
    /// <typeparam name="TInitiator">事务发起方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorAllParticipantPreCommitSucceedBase<TInitiator, TAggregateRootId>
        : DomainEvent<TAggregateRootId>
        , ITransactionInitiatorAllParticipantPreCommitSucceed<TInitiator>
        where TInitiator : AggregateRoot<TAggregateRootId>, ITransactionInitiator
    {
        /// <summary>
        /// 事务参与方所有预提交已成功事件基类
        /// </summary>
        protected TransactionInitiatorAllParticipantPreCommitSucceedBase() { }

        /// <summary>
        /// 事务参与方所有预提交已成功事件基类
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="transactionParticipants">事务参与者列表</param>
        protected TransactionInitiatorAllParticipantPreCommitSucceedBase(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> transactionParticipants)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            TransactionParticipants = transactionParticipants;
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
        /// 事务参与者列表
        /// </summary>
        public IEnumerable<TransactionParticipantInfo> TransactionParticipants { get; private set; }
    }
}
