using System;

namespace BankTransferSample.Domain
{
    /// <summary>
    /// 实体，表示账户聚合内的一笔预操作（如预存款、预转入）
    /// </summary>
    [Serializable]
    public class DepositTransactionPreparation : Eventual2PC.TransactionPreparationBase
    {
        /// <summary>
        /// 交易金额
        /// </summary>
        public double Amount { get; private set; }

        public DepositTransactionPreparation(string accountId, string transactionId, byte transactionType, double amount)
        : base(accountId, (byte)AggregateRootTypes.BankAccount, transactionId, transactionType, transactionId, (byte)AggregateRootTypes.TransferTransaction)
        {
            Amount = amount;
        }
    }
}
