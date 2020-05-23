using System;
using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方预提交成功事件基类
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    /// <typeparam name="TTransactionPreparation">事务准备</typeparam>
    [Serializable]
    public abstract class TransactionParticipantPreCommitSucceedBase<TParticipant, TAggregateRootId, TTransactionPreparation>
        : DomainEvent<TAggregateRootId>
        , ITransactionParticipantPreCommitSucceed<TParticipant, TTransactionPreparation>
        where TParticipant : AggregateRoot<TAggregateRootId>, ITransactionParticipant
        where TTransactionPreparation : class, ITransactionPreparation
    {
        /// <summary>
        /// 事务参与方预提交成功事件基类
        /// </summary>
        protected TransactionParticipantPreCommitSucceedBase() { }

        /// <summary>
        /// 事务参与方预提交成功事件基类
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        protected TransactionParticipantPreCommitSucceedBase(TTransactionPreparation transactionPreparation)
        {
            TransactionPreparation = transactionPreparation;
        }

        /// <summary>
        /// 事务准备
        /// </summary>
        public TTransactionPreparation TransactionPreparation { get; private set; }
    }
}
