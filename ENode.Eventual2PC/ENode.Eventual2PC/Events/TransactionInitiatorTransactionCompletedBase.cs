using System;
using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务已完成事件基类
    /// </summary>
    /// <typeparam name="TInitiator">事务发起方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorTransactionCompletedBase<TInitiator, TAggregateRootId>
        : DomainEvent<TAggregateRootId>
        , ITransactionInitiatorTransactionCompleted<TInitiator>
        where TInitiator : AggregateRoot<TAggregateRootId>, ITransactionInitiator
    {
        /// <summary>
        /// 事务已完成事件基类
        /// </summary>
        protected TransactionInitiatorTransactionCompletedBase() { }

        /// <summary>
        /// 事务已完成事件基类
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="isCommitSuccess">事务是否成功</param>
        protected TransactionInitiatorTransactionCompletedBase(string transactionId, byte transactionType, bool isCommitSuccess)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            IsCommitSuccess = isCommitSuccess;
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
        /// 事务是否成功
        /// </summary>
        public bool IsCommitSuccess  { get; private set; }
    }
}
