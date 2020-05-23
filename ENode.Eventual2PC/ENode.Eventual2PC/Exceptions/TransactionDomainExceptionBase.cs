using ENode.Domain;
using Eventual2PC;
using Eventual2PC.Events;
using System;
using System.Collections.Generic;

namespace ENode.Eventual2PC.Exceptions
{
    /// <summary>
    /// 事务领域异常基类
    /// </summary>
    /// <typeparam name="TParticipant"></typeparam>
    /// <typeparam name="TAggregateRootId"></typeparam>
    [Serializable]
    public abstract class TransactionDomainExceptionBase<TParticipant, TAggregateRootId>
        : DomainException, ITransactionParticipantPreCommitFailed<TParticipant>
        where TParticipant : AggregateRoot<TAggregateRootId>, ITransactionParticipant
    {
        /// <summary>
        /// 事务领域异常基类
        /// </summary>
        /// <param name="transactionPreparationType"></param>
        /// <param name="transactionPreparation"></param>
        public TransactionDomainExceptionBase(string transactionPreparationType, TransactionPreparationInfo transactionPreparation)
            : base()
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

        /// <summary>
        /// Serialize the current exception info to the given dictionary.
        /// </summary>
        /// <param name="serializableInfo"></param>
        public override void SerializeTo(IDictionary<string, string> serializableInfo)
        {
            serializableInfo.Add("TransactionPreparationType", TransactionPreparationType);
            serializableInfo.Add("ParticipantId", TransactionPreparation.ParticipantId);
            serializableInfo.Add("ParticipantType", TransactionPreparation.ParticipantType.ToString());
            serializableInfo.Add("TransactionId", TransactionPreparation.TransactionId);
            serializableInfo.Add("TransactionType", TransactionPreparation.TransactionType.ToString());
            serializableInfo.Add("InitiatorId", TransactionPreparation.InitiatorId);
            serializableInfo.Add("InitiatorType", TransactionPreparation.InitiatorType.ToString());
        }

        /// <summary>
        /// Restore the current exception from the given dictionary.
        /// </summary>
        /// <param name="serializableInfo"></param>
        public override void RestoreFrom(IDictionary<string, string> serializableInfo)
        {
            TransactionPreparationType = serializableInfo["TransactionPreparationType"];
            TransactionPreparation = new TransactionPreparationInfo
            (
                serializableInfo["ParticipantId"],
                byte.Parse(serializableInfo["ParticipantType"]),
                serializableInfo["TransactionId"],
                byte.Parse(serializableInfo["TransactionType"]),
                serializableInfo["InitiatorId"],
                byte.Parse(serializableInfo["InitiatorType"])
            );
        }
    }
}