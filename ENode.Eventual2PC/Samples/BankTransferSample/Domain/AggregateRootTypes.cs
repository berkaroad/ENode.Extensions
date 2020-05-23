namespace BankTransferSample.Domain
{
    public enum AggregateRootTypes : byte
    {
        BankAccount = 1,
        DepositTransaction = 2,
        TransferTransaction = 3
    }
}
