using System;
using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方已回滚事件基类
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    /// <typeparam name="TTransactionPreparation">事务准备</typeparam>
    [Serializable]
    public abstract class TransactionParticipantRolledbackBase<TParticipant, TAggregateRootId, TTransactionPreparation>
        : DomainEvent<TAggregateRootId>
        , ITransactionParticipantRolledback<TParticipant, TTransactionPreparation>
        where TParticipant : AggregateRoot<TAggregateRootId>, ITransactionParticipant
        where TTransactionPreparation : class, ITransactionPreparation
    {
        /// <summary>
        /// 事务参与方已回滚事件基类
        /// </summary>
        protected TransactionParticipantRolledbackBase() { }

        /// <summary>
        /// 事务参与方已回滚事件基类
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        protected TransactionParticipantRolledbackBase(TTransactionPreparation transactionPreparation)
        {
            TransactionPreparation = transactionPreparation;
        }

        /// <summary>
        /// 事务准备
        /// </summary>
        public TTransactionPreparation TransactionPreparation { get; private set; }
    }
}
