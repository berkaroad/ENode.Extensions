using BankTransferSample.Domain;
using ENode.Messaging;

namespace BankTransferSample.ApplicationMessages
{
    /// <summary>账户验证已通过
    /// </summary>
    public class AccountValidatePassedMessage : ApplicationMessage
    {
        public string AccountId { get; set; }
        public string TransactionId { get; set; }
        public byte TransactionType { get; set; }

        public PreparationTypes PreparationType { get; set; }
        
        public double Amount { get; set; }

        public AccountValidatePassedMessage() { }
        public AccountValidatePassedMessage(string accountId, string transactionId, byte transactionType, PreparationTypes preparationType, double amount)
        {
            AccountId = accountId;
            TransactionId = transactionId;
            TransactionType = transactionType;
            PreparationType = preparationType;
            Amount = amount;
        }
    }
}
