using System.Collections.Generic;
using Eventual2PC;

namespace BankTransferSample.Domain
{
    public class InsufficientBalanceDomainException : ENode.Eventual2PC.Exceptions.TransactionDomainExceptionBase<BankAccount, string>
    {
        /// <summary>
        /// 交易金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>
        /// 当前余额
        /// </summary>
        public double CurrentBalance { get; private set; }
        /// <summary>
        /// 当前可用余额
        /// </summary>
        public double CurrentAvailableBalance { get; private set; }

        public InsufficientBalanceDomainException(string transactionPreparationType, TransactionPreparationInfo transactionPreparation, double amount, double currentBalance, double currentAvailableBalance)
            : base(transactionPreparationType, transactionPreparation)
        {
            Amount = amount;
            CurrentBalance = currentBalance;
            CurrentAvailableBalance = currentAvailableBalance;
        }

        public override void SerializeTo(IDictionary<string, string> serializableInfo)
        {
            base.SerializeTo(serializableInfo);
            serializableInfo.Add("Amount", Amount.ToString());
            serializableInfo.Add("CurrentBalance", CurrentBalance.ToString());
            serializableInfo.Add("CurrentAvailableBalance", CurrentAvailableBalance.ToString());
        }
        public override void RestoreFrom(IDictionary<string, string> serializableInfo)
        {
            base.RestoreFrom(serializableInfo);
            Amount = double.Parse(serializableInfo["Amount"]);
            CurrentBalance = double.Parse(serializableInfo["CurrentBalance"]);
            CurrentAvailableBalance = double.Parse(serializableInfo["CurrentAvailableBalance"]);
        }
    }
}
