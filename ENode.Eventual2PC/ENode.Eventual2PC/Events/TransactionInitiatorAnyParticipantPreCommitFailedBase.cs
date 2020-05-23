using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;
using System;
using System.Collections.Generic;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方任意一个预提交已失败的事件基类
    /// </summary>
    /// <typeparam name="TInitiator">事务发起方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorAnyParticipantPreCommitFailedBase<TInitiator, TAggregateRootId>
        : DomainEvent<TAggregateRootId>
        , ITransactionInitiatorAnyParticipantPreCommitFailed<TInitiator>
        where TInitiator : AggregateRoot<TAggregateRootId>, ITransactionInitiator
    {
        /// <summary>
        /// 事务参与方任意一个预提交已失败的事件基类
        /// </summary>
        protected TransactionInitiatorAnyParticipantPreCommitFailedBase() { }

        /// <summary>
        /// 事务参与方任意一个预提交已失败的事件基类
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="preCommitSucceedTransactionParticipants">成功预提交的事务参与方信息</param>
        protected TransactionInitiatorAnyParticipantPreCommitFailedBase(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSucceedTransactionParticipants)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            PreCommitSucceedTransactionParticipants = preCommitSucceedTransactionParticipants;
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
        /// 成功预提交的事务参与方信息
        /// </summary>
        public IEnumerable<TransactionParticipantInfo> PreCommitSucceedTransactionParticipants { get; private set; }
    }
}
