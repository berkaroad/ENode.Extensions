namespace BankTransferSample.Domain
{
    /// <summary>交易类型枚举
    /// </summary>
    public enum TransactionTypes : byte
    {
        None = 0,
        /// <summary>存款
        /// </summary>
        DepositTransaction = 1,
        /// <summary>取款
        /// </summary>
        WithdrawTransaction = 2,
        /// <summary>转账
        /// </summary>
        TransferTransaction = 3
    }
}
