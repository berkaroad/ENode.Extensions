using ENode.Eventual2PC.Exceptions;
using Eventual2PC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventual2PC
{
    /// <summary>
    /// 事务发起方兼其他事务参与方
    /// </summary>
    /// <typeparam name="TTransactionInitiator">事务发起方实现类</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID类型</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorAlsoActAsOtherParticipantBase<TTransactionInitiator, TAggregateRootId>
        : TransactionInitiatorBase<TTransactionInitiator, TAggregateRootId>, ITransactionInitiator
        , ITransactionParticipant
        where TTransactionInitiator : TransactionInitiatorAlsoActAsOtherParticipantBase<TTransactionInitiator, TAggregateRootId>
    {
        private Dictionary<string, ITransactionPreparation> _transactionPreparations;

        /// <summary>
        /// 事务发起方兼其他事务参与方
        /// </summary>
        /// <returns></returns>
        protected TransactionInitiatorAlsoActAsOtherParticipantBase() : base() { }

        /// <summary>
        /// 事务发起方兼其他事务参与方
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected TransactionInitiatorAlsoActAsOtherParticipantBase(TAggregateRootId id) : base(id) { }

        /// <summary>
        /// 事务发起方兼其他事务参与方
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        protected TransactionInitiatorAlsoActAsOtherParticipantBase(TAggregateRootId id, int version) : base(id, version) { }

        /// <summary>
        /// 支持的事务准备类型列表
        /// </summary>
        protected abstract IEnumerable<Type> SupportedTransactionParticipantTypes { get; }

        /// <summary>
        /// 如果事务在处理中，是否阻止预提交（默认阻止）
        /// </summary>
        protected virtual bool PreventPreCommitOrNotIfTransactionProcessing => true;

        /// <summary>
        /// 预提交
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        public void PreCommit(ITransactionPreparation transactionPreparation)
        {
            if (transactionPreparation == null)
            {
                throw new ArgumentNullException(nameof(transactionPreparation));
            }
            if (SupportedTransactionParticipantTypes == null || !SupportedTransactionParticipantTypes.Contains(transactionPreparation.GetType()))
            {
                throw new ApplicationException($"Unknown transaction preparation {transactionPreparation.GetType().Name} for aggregate root {this.GetType().Name}, id={Id}.");
            }
            if (PreventPreCommitOrNotIfTransactionProcessing && IsTransactionProcessing)
            {
                throw new AlreadyStartTransactionWhenPreCommitDomainException<TTransactionInitiator, TAggregateRootId>(transactionPreparation.GetType().FullName, transactionPreparation.GetTransactionPreparationInfo());
            }
            InternalPreCommit(transactionPreparation);
        }

        /// <summary>
        /// 预提交
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        protected abstract void InternalPreCommit(ITransactionPreparation transactionPreparation);

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        public abstract void Commit(string transactionId);

        /// <summary>
        /// 回滚
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        public abstract void Rollback(string transactionId);

        /// <summary>
        /// 添加事务准备
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        protected void AddTransactionPreparation(ITransactionPreparation transactionPreparation)
        {
            if (_transactionPreparations == null)
            {
                _transactionPreparations = new Dictionary<string, ITransactionPreparation>();
            }
            _transactionPreparations.Add(transactionPreparation.TransactionId, transactionPreparation);
        }

        /// <summary>
        /// 获取事务准备
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <returns></returns>
        protected ITransactionPreparation GetTransactionPreparation(string transactionId)
        {
            return _transactionPreparations != null && _transactionPreparations.ContainsKey(transactionId) ? _transactionPreparations[transactionId] : null;
        }

        /// <summary>
        /// 获取指定类型的事务准备列表
        /// </summary>
        /// <typeparam name="TTransactionPreparation">具体的事务准备</typeparam>
        /// <returns></returns>
        protected IReadOnlyList<TTransactionPreparation> GetTransactionPreparationList<TTransactionPreparation>()
            where TTransactionPreparation : class, ITransactionPreparation
        {
            return _transactionPreparations == null ? new List<TTransactionPreparation>().AsReadOnly() : _transactionPreparations.Values.Select(s => s as TTransactionPreparation).Where(w => w != null).ToList().AsReadOnly();
        }

        /// <summary>
        /// 获取所有事务准备
        /// </summary>
        /// <returns></returns>
        protected IReadOnlyList<ITransactionPreparation> GetAllTransactionPreparations()
        {
            return _transactionPreparations == null ? new List<ITransactionPreparation>().AsReadOnly() : _transactionPreparations.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// 移除事务准备
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        protected void RemoveTransactionPreparation(string transactionId)
        {
            if (_transactionPreparations != null && _transactionPreparations.ContainsKey(transactionId))
            {
                _transactionPreparations.Remove(transactionId);
            }
        }

        /// <summary>
        /// 判断指定的事务准备类型，是否已存在（不同事务流程可能存在互斥，即不能同时存在）
        /// </summary>
        /// <typeparam name="TTransactionPreparation">具体的事务准备</typeparam>
        /// <returns></returns>
        protected bool IsSpecificTransactionPreparationTypeExists<TTransactionPreparation>()
            where TTransactionPreparation : ITransactionPreparation
        {
            return _transactionPreparations != null && _transactionPreparations.Values.Any(a => a.GetType() == typeof(TTransactionPreparation));
        }
    }
}
