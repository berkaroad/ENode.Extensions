using System;
using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方已提交事件基类
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    /// <typeparam name="TTransactionPreparation">事务准备</typeparam>
    [Serializable]
    public abstract class TransactionParticipantCommittedBase<TParticipant, TAggregateRootId, TTransactionPreparation>
        : DomainEvent<TAggregateRootId>
        , ITransactionParticipantCommitted<TParticipant, TTransactionPreparation>
        where TParticipant : AggregateRoot<TAggregateRootId>, ITransactionParticipant
        where TTransactionPreparation : class, ITransactionPreparation
    {
        /// <summary>
        /// 事务参与方已提交事件基类
        /// </summary>
        protected TransactionParticipantCommittedBase() { }

        /// <summary>
        /// 事务参与方已提交事件基类
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        protected TransactionParticipantCommittedBase(TTransactionPreparation transactionPreparation)
        {
            TransactionPreparation = transactionPreparation;
        }

        /// <summary>
        /// 事务准备
        /// </summary>
        public TTransactionPreparation TransactionPreparation { get; private set; }
    }
}
