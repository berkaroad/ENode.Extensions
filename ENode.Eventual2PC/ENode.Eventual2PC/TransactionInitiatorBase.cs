using ENode.Domain;
using ENode.Eventual2PC.Events;
using Eventual2PC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventual2PC
{
    /// <summary>
    /// 事务发起方
    /// </summary>
    /// <typeparam name="TTransactionInitiator">事务发起方实现类</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID类型</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorBase<TTransactionInitiator, TAggregateRootId>
        : AggregateRoot<TAggregateRootId>, ITransactionInitiator
        where TTransactionInitiator : TransactionInitiatorBase<TTransactionInitiator, TAggregateRootId>
    {
        private List<TransactionParticipantInfo> _allTransactionParticipants;
        private List<TransactionParticipantInfo> _preCommitSuccessTransactionParticipants;
        private List<TransactionParticipantInfo> _preCommitFailTransactionParticipants;
        private List<TransactionParticipantInfo> _committedTransactionParticipants;
        private List<TransactionParticipantInfo> _rolledbackTransactionParticipants;

        /// <summary>
        /// 事务发起方
        /// </summary>
        /// <returns></returns>
        protected TransactionInitiatorBase() : base() { }

        /// <summary>
        /// 事务发起方
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected TransactionInitiatorBase(TAggregateRootId id) : base(id) { }

        /// <summary>
        /// 事务发起方
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        protected TransactionInitiatorBase(TAggregateRootId id, int version) : base(id, version) { }

        /// <summary>
        /// 是否事务处理中
        /// </summary>
        public bool IsTransactionProcessing { get; private set; }

        /// <summary>
        /// 当前事务ID
        /// </summary>
        public string CurrentTransactionId { get; private set; }

        /// <summary>
        /// 当前事务类型
        /// </summary>
        public byte CurrentTransactionType { get; private set; }

        /// <summary>
        /// 是否所有预提交参与者都已添加且都成功
        /// </summary>
        /// <returns></returns>
        protected bool IsAllPreCommitParticipantAddedAndSuccess()
        {
            return _allTransactionParticipants.Count == _preCommitSuccessTransactionParticipants.Count;
        }

        /// <summary>
        /// 是否所有预提交参与者都已添加且都失败
        /// </summary>
        /// <returns></returns>
        protected bool IsAllPreCommitParticipantAddedAndFail()
        {
            return _allTransactionParticipants.Count == _preCommitFailTransactionParticipants.Count;
        }

        /// <summary>
        /// 是否所有预提交参与者都已添加
        /// </summary>
        /// <returns></returns>
        protected bool IsAllPreCommitParticipantAdded()
        {
            return _allTransactionParticipants.Count == _preCommitSuccessTransactionParticipants.Count + _preCommitFailTransactionParticipants.Count;
        }

        /// <summary>
        /// 是否所有已提交和已回滚的参与者都已添加
        /// </summary>
        /// <returns></returns>
        protected bool IsAllCommittedAndRolledbackParticipantAdded()
        {
            return _allTransactionParticipants.Count == _committedTransactionParticipants.Count
                || _preCommitSuccessTransactionParticipants.Count == _rolledbackTransactionParticipants.Count;
        }

        /// <summary>
        /// 添加预提交成功的参与者（依次发布 PreCommitSuccessParticipantAdded 、 AllParticipantPreCommitSucceed（或 AnyParticipantPreCommitFailed） 事件）
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        public void AddPreCommitSuccessParticipant(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            if (participantInfo == null)
            {
                throw new ArgumentNullException(nameof(participantInfo));
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (!IsTransactionProcessing)
            {
                throw new ApplicationException($"Initiator {Id} is not in transaction, couldn't execute AddPreCommitSuccessParticipant command.");
            }
            if (transactionType != CurrentTransactionType)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction type {transactionType} is not same as {CurrentTransactionType}");
            }
            if (!string.IsNullOrEmpty(CurrentTransactionId) && transactionId != CurrentTransactionId)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction id {transactionId} is not same as {CurrentTransactionId}");
            }
            if (_preCommitSuccessTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_preCommitSuccessTransactionParticipants))
            {
                return;
            }
            if (_preCommitFailTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_preCommitFailTransactionParticipants))
            {
                return;
            }
            if (_allTransactionParticipants == null || !_allTransactionParticipants.Any(w => w.ParticipantId == participantInfo.ParticipantId))
            {
                return;
            }
            ApplyEvent(CreatePreCommitSuccessParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllPreCommitParticipantAddedAndSuccess())
            {
                // 所有参与者的预提交都已成功处理
                ApplyEvent(CreateAllParticipantPreCommitSucceedEvent(transactionId, transactionType, _preCommitSuccessTransactionParticipants));
            }
            else if (IsAllPreCommitParticipantAdded())
            {
                // 所有预提交已添加
                ApplyEvent(CreateAnyParticipantPreCommitFailedEvent(transactionId, transactionType, _preCommitSuccessTransactionParticipants));
            }
        }

        /// <summary>
        /// 添加预提交失败的参与者（依次发布 PreCommitFailParticipantAdded 、 AnyParticipantPreCommitFailed 事件）
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        public void AddPreCommitFailedParticipant(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            if (participantInfo == null)
            {
                throw new ArgumentNullException(nameof(participantInfo));
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (!IsTransactionProcessing)
            {
                throw new ApplicationException($"Initiator {Id} is not in transaction, couldn't execute AddPreCommitFailedParticipant command.");
            }
            if (transactionType != CurrentTransactionType)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction type {transactionType} is not same as {CurrentTransactionType}");
            }
            if (!string.IsNullOrEmpty(CurrentTransactionId) && transactionId != CurrentTransactionId)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction id {transactionId} is not same as {CurrentTransactionId}");
            }
            if (_preCommitSuccessTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_preCommitSuccessTransactionParticipants))
            {
                return;
            }
            if (_preCommitFailTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_preCommitFailTransactionParticipants))
            {
                return;
            }
            if (_allTransactionParticipants == null || !_allTransactionParticipants.Any(w => w.ParticipantId == participantInfo.ParticipantId))
            {
                return;
            }

            ApplyEvent(CreatePreCommitFailParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllPreCommitParticipantAdded())
            {
                // 所有预提交已添加
                ApplyEvent(CreateAnyParticipantPreCommitFailedEvent(transactionId, transactionType, _preCommitSuccessTransactionParticipants));
                if (IsAllPreCommitParticipantAddedAndFail())
                {
                    // 所有预提交已添加且都失败，事务直接完成
                    ApplyEvent(CreateTransactionCompletedEvent(transactionId, transactionType, false));
                }
            }
        }

        /// <summary>
        /// 添加已提交的参与者（依次发布 CommittedParticipantAdded 、 TransactionCompleted 事件）
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        public void AddCommittedParticipant(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            if (participantInfo == null)
            {
                throw new ArgumentNullException(nameof(participantInfo));
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (!IsTransactionProcessing)
            {
                throw new ApplicationException($"Initiator {Id} is not in transaction, couldn't execute AddCommittedParticipant command.");
            }
            if (transactionType != CurrentTransactionType)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction type {transactionType} is not same as {CurrentTransactionType}");
            }
            if (!string.IsNullOrEmpty(CurrentTransactionId) && transactionId != CurrentTransactionId)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction id {transactionId} is not same as {CurrentTransactionId}");
            }
            if (_committedTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_committedTransactionParticipants))
            {
                return;
            }
            if (_rolledbackTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_rolledbackTransactionParticipants))
            {
                return;
            }
            if (!IsAllPreCommitParticipantAdded())
            {
                throw new ApplicationException("Initiator {Id} didn't received all PreCommit participant, couldn't execute AddCommittedParticipant command.");
            }
            if (_allTransactionParticipants == null || !_allTransactionParticipants.Any(w => w.ParticipantId == participantInfo.ParticipantId))
            {
                return;
            }

            ApplyEvent(CreateCommittedParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllCommittedAndRolledbackParticipantAdded())
            {
                // 所有参与者的提交和回滚都已处理
                ApplyEvent(CreateTransactionCompletedEvent(transactionId, transactionType, _rolledbackTransactionParticipants.Count == 0));
            }
        }

        /// <summary>
        /// 添加已回滚的参与者（依次发布 RolledbackParticipantAdded 、 TransactionCompleted 事件）
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        public void AddRolledbackParticipant(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
        {
            if (participantInfo == null)
            {
                throw new ArgumentNullException(nameof(participantInfo));
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (!IsTransactionProcessing)
            {
                throw new ApplicationException($"Initiator {Id} is not in transaction, couldn't execute AddCommittedParticipant command.");
            }
            if (transactionType != CurrentTransactionType)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction type {transactionType} is not same as {CurrentTransactionType}");
            }
            if (!string.IsNullOrEmpty(CurrentTransactionId) && transactionId != CurrentTransactionId)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction id {transactionId} is not same as {CurrentTransactionId}");
            }
            if (_committedTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_committedTransactionParticipants))
            {
                return;
            }
            if (_rolledbackTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_rolledbackTransactionParticipants))
            {
                return;
            }
            if (!IsAllPreCommitParticipantAdded())
            {
                throw new ApplicationException("Initiator {Id} didn't received all PreCommit participant, couldn't execute AddRolledbackParticipant command.");
            }
            if (_allTransactionParticipants == null || !_allTransactionParticipants.Any(w => w.ParticipantId == participantInfo.ParticipantId))
            {
                return;
            }

            ApplyEvent(CreateRolledbackParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllCommittedAndRolledbackParticipantAdded())
            {
                // 所有参与者的提交和回滚都已处理
                ApplyEvent(CreateTransactionCompletedEvent(transactionId, transactionType, _rolledbackTransactionParticipants.Count == 0));
            }
        }

        /// <summary>
        /// Create PreCommitSuccessParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        protected abstract TransactionInitiatorPreCommitSucceedParticipantAddedBase<TTransactionInitiator, TAggregateRootId> CreatePreCommitSuccessParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create PreCommitFailParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        protected abstract TransactionInitiatorPreCommitFailedParticipantAddedBase<TTransactionInitiator, TAggregateRootId> CreatePreCommitFailParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create AllParticipantPreCommitSucceed event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="preCommitSuccessTransactionParticipants"></param>
        protected abstract TransactionInitiatorAllParticipantPreCommitSucceedBase<TTransactionInitiator, TAggregateRootId> CreateAllParticipantPreCommitSucceedEvent(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSuccessTransactionParticipants);

        /// <summary>
        /// Create AnyParticipantPreCommitFailed event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="preCommitSuccessTransactionParticipants"></param>
        protected abstract TransactionInitiatorAnyParticipantPreCommitFailedBase<TTransactionInitiator, TAggregateRootId> CreateAnyParticipantPreCommitFailedEvent(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSuccessTransactionParticipants);

        /// <summary>
        /// Create CommittedParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        protected abstract TransactionInitiatorCommittedParticipantAddedBase<TTransactionInitiator, TAggregateRootId> CreateCommittedParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create RolledbackParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        protected abstract TransactionInitiatorRolledbackParticipantAddedBase<TTransactionInitiator, TAggregateRootId> CreateRolledbackParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create TransactionCompleted event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="isCommitSuccess"></param>
        protected abstract TransactionInitiatorTransactionCompletedBase<TTransactionInitiator, TAggregateRootId> CreateTransactionCompletedEvent(string transactionId, byte transactionType, bool isCommitSuccess);

        /// <summary>
        /// Handle TransactionStarted event
        /// </summary>
        /// <param name="transactionType">事务类型</param>
        /// <param name="allTransactionParticipants"></param>
        protected void HandleTransactionStartedEvent(byte transactionType, IEnumerable<TransactionParticipantInfo> allTransactionParticipants)
        {
            IsTransactionProcessing = true;
            CurrentTransactionType = transactionType;
            _allTransactionParticipants = new List<TransactionParticipantInfo>();
            if (allTransactionParticipants == null || !allTransactionParticipants.Any())
            {
                throw new ApplicationException($"Initiator {Id} hasn't any participant in transaction [{transactionType}].");
            }
            if (allTransactionParticipants.Any(w => w.ParticipantId == Id.ToString()))
            {
                throw new ApplicationException($"Initiator {Id} cann't act as participant in transaction [{transactionType}].");
            }
            _allTransactionParticipants.AddRange(allTransactionParticipants);
            _preCommitSuccessTransactionParticipants = new List<TransactionParticipantInfo>();
            _preCommitFailTransactionParticipants = new List<TransactionParticipantInfo>();
            _committedTransactionParticipants = new List<TransactionParticipantInfo>();
            _rolledbackTransactionParticipants = new List<TransactionParticipantInfo>();
        }

        /// <summary>
        /// Handle PreCommitSuccessParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected void HandlePreCommitSuccessParticipantAddedEvent(string transactionId, TransactionParticipantInfo transactionParticipant)
        {
            CurrentTransactionId = transactionId;
            _preCommitSuccessTransactionParticipants.Add(transactionParticipant);
        }

        /// <summary>
        /// Handle PreCommitFailParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected void HandlePreCommitFailParticipantAddedEvent(string transactionId, TransactionParticipantInfo transactionParticipant)
        {
            CurrentTransactionId = transactionId;
            _preCommitFailTransactionParticipants.Add(transactionParticipant);
        }

        /// <summary>
        /// Handle CommittedParticipantAdded event
        /// </summary>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected void HandleCommittedParticipantAddedEvent(TransactionParticipantInfo transactionParticipant)
        {
            _committedTransactionParticipants.Add(transactionParticipant);
        }

        /// <summary>
        /// Handle RolledbackParticipantAdded event
        /// </summary>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected void HandleRolledbackParticipantAddedEvent(TransactionParticipantInfo transactionParticipant)
        {
            _rolledbackTransactionParticipants.Add(transactionParticipant);
        }

        /// <summary>
        /// Handle TransactionCompleted event
        /// </summary>
        protected void HandleTransactionCompletedEvent()
        {
            IsTransactionProcessing = false;
            CurrentTransactionType = 0;
            CurrentTransactionId = string.Empty;
            _allTransactionParticipants.Clear();
            _preCommitSuccessTransactionParticipants.Clear();
            _preCommitFailTransactionParticipants.Clear();
            _committedTransactionParticipants.Clear();
            _rolledbackTransactionParticipants.Clear();
        }
    }
}
