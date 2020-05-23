using ENode.Eventual2PC.Events;
using Eventual2PC;
using System.Collections.Generic;

namespace BankTransferSample.Domain
{
    /// <summary>
    /// 转账交易
    /// </summary>
    public class TransferTransaction : ENode.Eventual2PC.TransactionInitiatorBase<TransferTransaction,string>
    {
        #region Private Variables

        private TransferTransactionInfo _transactionInfo;

        #endregion

        /// <summary>
        /// 交易状态
        /// </summary>
        public TransactionStatus Status { get; private set; }

        #region Constructors

        /// <summary>
        /// 转账交易
        /// </summary>
        public TransferTransaction(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId)
        {
            ApplyEvent(new TransferTransactionStartedEvent(transactionInfo));
        }

        #endregion

        #region Public Methods

        protected override TransactionInitiatorPreCommitSucceedParticipantAddedBase<TransferTransaction, string> CreatePreCommitSuccessParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            var preparationType=_transactionInfo.SourceAccountId == participantInfo.ParticipantId ? PreparationTypes.DebitPreparation : PreparationTypes.CreditPreparation;
            return new TransferTransactionPreCommitSucceedParticipantAdded(participantInfo.ParticipantId, transactionId, preparationType);
        }

        protected override TransactionInitiatorPreCommitFailedParticipantAddedBase<TransferTransaction, string> CreatePreCommitFailParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            return new TransferTransactionPreCommitFailedParticipantAdded(participantInfo.ParticipantId, transactionId);
        }

        protected override TransactionInitiatorAllParticipantPreCommitSucceedBase<TransferTransaction, string> CreateAllParticipantPreCommitSucceedEvent(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSuccessTransactionParticipants)
        {
            return new TransferTransactionAllParticipantPreCommitSucceedEvent(transactionId, preCommitSuccessTransactionParticipants);
        }

        protected override TransactionInitiatorAnyParticipantPreCommitFailedBase<TransferTransaction, string> CreateAnyParticipantPreCommitFailedEvent(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSuccessTransactionParticipants)
        {
            return new TransferTransactionAnyParticipantPreCommitFailedEvent(transactionId, preCommitSuccessTransactionParticipants);
        }

        protected override TransactionInitiatorCommittedParticipantAddedBase<TransferTransaction, string> CreateCommittedParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            return new TransferTransactionCommittedParticipantAddedEvent(participantInfo.ParticipantId, transactionId);
        }

        protected override TransactionInitiatorRolledbackParticipantAddedBase<TransferTransaction, string> CreateRolledbackParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            return new TransferTransactionRolledbackParticipantAddedEvent(participantInfo.ParticipantId, transactionId);
        }

        protected override TransactionInitiatorTransactionCompletedBase<TransferTransaction, string> CreateTransactionCompletedEvent(string transactionId, byte transactionType, bool isCommitSuccess)
        {
            return new TransferTransactionCompletedEvent(transactionId, isCommitSuccess);
        }

        #endregion

        #region Handler Methods

        private void Handle(TransferTransactionStartedEvent evnt)
        {
            _transactionInfo = evnt.TransactionInfo;
            Status = TransactionStatus.Started;
            var participants = new TransactionParticipantInfo[] {
                new TransactionParticipantInfo(_transactionInfo.SourceAccountId, (byte)AggregateRootTypes.BankAccount),
                new TransactionParticipantInfo(_transactionInfo.TargetAccountId, (byte)AggregateRootTypes.BankAccount),
            };
            HandleTransactionStartedEvent((byte)TransactionTypes.TransferTransaction, participants);
        }

        private void Handle(TransferTransactionPreCommitSucceedParticipantAdded evnt)
        {
            HandlePreCommitSuccessParticipantAddedEvent(evnt.TransactionId, evnt.TransactionParticipant);
        }
        private void Handle(TransferTransactionPreCommitFailedParticipantAdded evnt)
        {
            HandlePreCommitFailParticipantAddedEvent(evnt.TransactionId, evnt.TransactionParticipant);
        }
        private void Handle(TransferTransactionAnyParticipantPreCommitFailedEvent evnt)
        {
           // 存在准备失败的场景
        }
        private void Handle(TransferTransactionAllParticipantPreCommitSucceedEvent evnt)
        {
            Status = TransactionStatus.PreparationCompleted;
        }

        private void Handle(TransferTransactionCommittedParticipantAddedEvent evnt)
        {
            HandleCommittedParticipantAddedEvent(evnt.TransactionParticipant);
        }

        private void Handle(TransferTransactionRolledbackParticipantAddedEvent evnt)
        {
            HandleRolledbackParticipantAddedEvent(evnt.TransactionParticipant);
        }

        private void Handle(TransferTransactionCompletedEvent evnt)
        {
            Status = evnt.IsCommitSuccess ? TransactionStatus.Completed : TransactionStatus.Canceled;
            HandleTransactionCompletedEvent();
        }

        #endregion
    }
}
