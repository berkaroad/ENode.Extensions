using System;
using ENode.Domain;
using ENode.Eventing;
using Eventual2PC;
using Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方预提交失败事件基类
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    [Serializable]
    public abstract class TransactionParticipantPreCommitFailedBase<TParticipant, TAggregateRootId>
        : DomainEvent<TAggregateRootId>
        , ITransactionParticipantPreCommitFailed<TParticipant>
        where TParticipant : AggregateRoot<TAggregateRootId>, ITransactionParticipant
    {
        /// <summary>
        /// 事务参与方预提交失败事件基类
        /// </summary>
        protected TransactionParticipantPreCommitFailedBase() { }

        /// <summary>
        /// 事务参与方预提交失败事件基类
        /// </summary>
        /// <param name="transactionPreparationType">事务准备类型</param>
        /// <param name="transactionPreparation">事务准备</param>
        protected TransactionParticipantPreCommitFailedBase(string transactionPreparationType, TransactionPreparationInfo transactionPreparation)
        {
            TransactionPreparationType = transactionPreparationType;
            TransactionPreparation = transactionPreparation;
        }

        /// <summary>
        /// 事务准备类型
        /// </summary>
        public string TransactionPreparationType { get; private set; }

        /// <summary>
        /// 事务准备
        /// </summary>
        public TransactionPreparationInfo TransactionPreparation { get; private set; }
    }
}
