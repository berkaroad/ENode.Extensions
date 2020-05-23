using System;

namespace BankTransferSample.Domain
{
    /// <summary>实体，表示账户聚合内的一笔预操作（如预取款、预转出）
    /// </summary>
    [Serializable]
    public class WithdrawTransactionPreparation : Eventual2PC.TransactionPreparationBase
    {
        /// <summary>交易金额
        /// </summary>
        public double Amount { get; private set; }

        public WithdrawTransactionPreparation(string accountId, string transactionId, byte transactionType, double amount)
            :base(accountId, (byte)AggregateRootTypes.BankAccount, transactionId, transactionType, transactionId, (byte)AggregateRootTypes.TransferTransaction)
        {   
            Amount = amount;
        }
    }
}
