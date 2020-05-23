using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;
using ENode.Eventual2PC.Events;
using ENode.Infrastructure;
using Eventual2PC;

namespace BankTransferSample.Domain
{
    /// <summary>聚合根，表示一笔银行存款交易
    /// </summary>
    public class DepositTransaction : ENode.Eventual2PC.TransactionInitiatorBase<DepositTransaction, string>
    {
        #region Private Variables

        private string _accountId;
        private double _amount;

        public TransactionStatus Status { get; private set; }

        #endregion

        #region Constructors

        /// <summary>构造函数
        /// </summary>
        public DepositTransaction(string transactionId, string accountId, double amount)
            : base(transactionId)
        {
            ApplyEvent(new DepositTransactionStartedEvent(accountId, amount));
        }

        #endregion

        #region Private Methods

        protected override TransactionInitiatorPreCommitSucceedParticipantAddedBase<DepositTransaction, string> CreatePreCommitSuccessParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            return new DepositTransactionPreCommitSucceedParticipantAdded(participantInfo.ParticipantId, transactionId);
        }

        protected override TransactionInitiatorPreCommitFailedParticipantAddedBase<DepositTransaction, string> CreatePreCommitFailParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            //TODO: 存款，假定不存在需要回滚的场景
            throw new NotImplementedException();
        }

        protected override TransactionInitiatorAllParticipantPreCommitSucceedBase<DepositTransaction, string> CreateAllParticipantPreCommitSucceedEvent(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSuccessTransactionParticipants)
        {
            return new DepositTransactionAllParticipantPreCommitSucceedEvent(preCommitSuccessTransactionParticipants.First().ParticipantId, transactionId);
        }

        protected override TransactionInitiatorAnyParticipantPreCommitFailedBase<DepositTransaction, string> CreateAnyParticipantPreCommitFailedEvent(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSuccessTransactionParticipants)
        {
            //TODO: 存款，假定不存在需要回滚的场景
            throw new NotImplementedException();
        }

        protected override TransactionInitiatorCommittedParticipantAddedBase<DepositTransaction, string> CreateCommittedParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            return new DepositTransactionCommittedParticipantAddedEvent(participantInfo.ParticipantId, transactionId);
        }

        protected override TransactionInitiatorRolledbackParticipantAddedBase<DepositTransaction, string> CreateRolledbackParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            //TODO: 存款，假定不存在需要回滚的场景
            throw new NotImplementedException();
        }

        protected override TransactionInitiatorTransactionCompletedBase<DepositTransaction, string> CreateTransactionCompletedEvent(string transactionId, byte transactionType, bool isCommitSuccess)
        {
            return new DepositTransactionCompletedEvent(transactionId, isCommitSuccess);
        }

        #endregion

        #region Handler Methods

        private void Handle(DepositTransactionStartedEvent evnt)
        {
            _accountId = evnt.AccountId;
            _amount = evnt.Amount;
            Status = TransactionStatus.Started;
            var participants = new TransactionParticipantInfo[] {
                new TransactionParticipantInfo(evnt.AccountId, (byte)AggregateRootTypes.BankAccount)
            };
            HandleTransactionStartedEvent((byte)TransactionTypes.DepositTransaction, participants);
        }

        private void Handle(DepositTransactionPreCommitSucceedParticipantAdded evnt)
        {
            HandlePreCommitSuccessParticipantAddedEvent(evnt.TransactionId, evnt.TransactionParticipant);
        }

        private void Handle(DepositTransactionAllParticipantPreCommitSucceedEvent evnt)
        {
            Status = TransactionStatus.PreparationCompleted;
        }

        private void Handle(DepositTransactionCommittedParticipantAddedEvent evnt)
        {
            HandleCommittedParticipantAddedEvent(evnt.TransactionParticipant);
        }

        private void Handle(DepositTransactionCompletedEvent evnt)
        {
            Status = TransactionStatus.Completed;
            HandleTransactionCompletedEvent();
        }

        #endregion
    }
}
