using System;
using System.Collections.Generic;
using System.Linq;
using Eventual2PC;

namespace BankTransferSample.Domain
{
    /// <summary>
    /// 银行账户聚合根，封装银行账户余额变动的数据一致性
    /// </summary>
    public class BankAccount : ENode.Eventual2PC.TransactionParticipantBase<string>
    {
        #region Private Variables
        private string _owner;
        private double _balance;

        protected override IEnumerable<Type> SupportedTransactionParticipantTypes
            => new Type[] {
                typeof(WithdrawTransactionPreparation),
                typeof(DepositTransactionPreparation)
            };

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public BankAccount(string accountId, string owner) : base(accountId)
        {
            ApplyEvent(new AccountCreatedEvent(owner));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 添加一笔预操作
        /// </summary>
        /// <param name="transactionPreparation"></param>
        protected override void InternalPreCommit(ITransactionPreparation transactionPreparation)
        {
            var availableBalance = GetAvailableBalance();
            if(transactionPreparation is WithdrawTransactionPreparation)
            {
                var theTransactionPreparation = transactionPreparation as WithdrawTransactionPreparation;
                if (availableBalance < theTransactionPreparation.Amount)
                {
                    throw new InsufficientBalanceDomainException(theTransactionPreparation.GetType().FullName, theTransactionPreparation.GetTransactionPreparationInfo(), theTransactionPreparation.Amount, _balance, availableBalance);
                }
                ApplyEvent(new WithdrawTransactionPreCommitSucceedEvent(theTransactionPreparation));
            }
            else if(transactionPreparation is DepositTransactionPreparation)
            {
                var theTransactionPreparation = transactionPreparation as DepositTransactionPreparation;
                ApplyEvent(new DepositTransactionPreCommitSucceedEvent(theTransactionPreparation));
            }
        }

        /// <summary>
        /// 提交一笔预操作
        /// </summary>
        /// <param name="transactionId"></param>
        public override void Commit(string transactionId)
        {
            var transactionPreparation = GetTransactionPreparation(transactionId);
            var currentBalance = _balance;
            if (transactionPreparation is WithdrawTransactionPreparation)
            {
                var theTransactionPreparation = transactionPreparation as WithdrawTransactionPreparation;
                currentBalance -= theTransactionPreparation.Amount;
                ApplyEvent(new WithdrawTransactionCommittedEvent(currentBalance, theTransactionPreparation));
            }
            else if (transactionPreparation is DepositTransactionPreparation)
            {
                var theTransactionPreparation = transactionPreparation as DepositTransactionPreparation;
                currentBalance += theTransactionPreparation.Amount;
                ApplyEvent(new DepositTransactionCommittedEvent(currentBalance, theTransactionPreparation));
            }
        }

        /// <summary>
        /// 取消一笔预操作
        /// </summary>
        /// <param name="transactionId"></param>
        public override void Rollback(string transactionId)
        {
            var transactionPreparation = GetTransactionPreparation(transactionId);
            if (transactionPreparation is WithdrawTransactionPreparation)
            {
                var theTransactionPreparation = transactionPreparation as WithdrawTransactionPreparation;
                ApplyEvent(new WithdrawTransactionRolledbackEvent(theTransactionPreparation));
            }
            else if (transactionPreparation is DepositTransactionPreparation)
            {
                var theTransactionPreparation = transactionPreparation as DepositTransactionPreparation;
                ApplyEvent(new DepositTransactionRolledbackEvent(theTransactionPreparation));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 获取当前账户的可用余额，需要将已冻结的余额计算在内
        /// </summary>
        private double GetAvailableBalance()
        {
            var transactionPreparations = GetTransactionPreparationList<WithdrawTransactionPreparation>();
            if (transactionPreparations == null || transactionPreparations.Count == 0)
            {
                return _balance;
            }

            var totalDebitTransactionPreparationAmount = 0D;
            foreach (var debitTransactionPreparation in transactionPreparations)
            {
                totalDebitTransactionPreparationAmount += debitTransactionPreparation.Amount;
            }

            return _balance - totalDebitTransactionPreparationAmount;
        }

        #endregion

        #region Handler Methods

        private void Handle(AccountCreatedEvent evnt)
        {
            _owner = evnt.Owner;
        }
        private void Handle(WithdrawTransactionPreCommitSucceedEvent evnt)
        {
            AddTransactionPreparation(evnt.TransactionPreparation);
        }
        private void Handle(WithdrawTransactionCommittedEvent evnt)
        {
            _balance = evnt.CurrentBalance;
            RemoveTransactionPreparation(evnt.TransactionPreparation.TransactionId);
        }
        private void Handle(WithdrawTransactionRolledbackEvent evnt)
        {
            RemoveTransactionPreparation(evnt.TransactionPreparation.TransactionId);
        }

        private void Handle(DepositTransactionPreCommitSucceedEvent evnt)
        {
            AddTransactionPreparation(evnt.TransactionPreparation);
        }
        private void Handle(DepositTransactionCommittedEvent evnt)
        {
            _balance = evnt.CurrentBalance;
            RemoveTransactionPreparation(evnt.TransactionPreparation.TransactionId);
        }
        private void Handle(DepositTransactionRolledbackEvent evnt)
        {
            RemoveTransactionPreparation(evnt.TransactionPreparation.TransactionId);
        }

        #endregion
    }
}
