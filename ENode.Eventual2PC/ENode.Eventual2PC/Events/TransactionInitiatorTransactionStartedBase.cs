using System;
using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务已开始事件基类
    /// </summary>
    /// <typeparam name="TInitiator">事务发起方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorTransactionStartedBase<TInitiator, TAggregateRootId>
        : DomainEvent<TAggregateRootId>
        , ITransactionInitiatorTransactionStarted<TInitiator>
        where TInitiator : AggregateRoot<TAggregateRootId>, ITransactionInitiator
    {
        /// <summary>
        /// 事务已开始事件基类
        /// </summary>
        protected TransactionInitiatorTransactionStartedBase() { }

        /// <summary>
        /// 事务已开始事件基类
        /// </summary>
        /// <param name="transactionType">事务类型</param>
        protected TransactionInitiatorTransactionStartedBase(byte transactionType)
        {
            TransactionType = transactionType;
        }

        /// <summary>
        /// 事务ID
        /// </summary>
        public abstract string TransactionId { get; }

        /// <summary>
        /// 事务类型
        /// </summary>
        public byte TransactionType { get; private set; }
    }
}
