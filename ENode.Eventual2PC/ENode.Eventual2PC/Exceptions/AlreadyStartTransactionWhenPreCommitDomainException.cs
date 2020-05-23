using System;
using ENode.Domain;
using Eventual2PC;

namespace ENode.Eventual2PC.Exceptions
{
    /// <summary>
    /// 预提交时已经发起事务
    /// </summary>
    /// <typeparam name="TParticipant"></typeparam>
    /// <typeparam name="TAggregateRootId"></typeparam>
    [Serializable]
    public class AlreadyStartTransactionWhenPreCommitDomainException<TParticipant, TAggregateRootId>
        : TransactionDomainExceptionBase<TParticipant, TAggregateRootId>
        where TParticipant : AggregateRoot<TAggregateRootId>, ITransactionParticipant
    {
        /// <summary>
        /// 预提交时已经发起事务
        /// </summary>
        /// <param name="transactionPreparationType"></param>
        /// <param name="transactionPreparation"></param>
        public AlreadyStartTransactionWhenPreCommitDomainException(string transactionPreparationType, TransactionPreparationInfo transactionPreparation)
            : base(transactionPreparationType, transactionPreparation)
        { }
    }
}