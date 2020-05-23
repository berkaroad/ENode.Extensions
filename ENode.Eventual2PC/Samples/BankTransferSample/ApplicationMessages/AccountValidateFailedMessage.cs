using BankTransferSample.Domain;
using ENode.Messaging;

namespace BankTransferSample.ApplicationMessages
{
    /// <summary>账户验证未通过
    /// </summary>
    public class AccountValidateFailedMessage : ApplicationMessage
    {
        public string AccountId { get; set; }
        public string TransactionId { get; set; }
        public byte TransactionType { get; set; }

        public PreparationTypes PreparationType { get; set; }
        public string Reason { get; set; }

        public AccountValidateFailedMessage() { }
        public AccountValidateFailedMessage(string accountId, string transactionId, byte transactionType, PreparationTypes preparationType, string reason)
        {
            AccountId = accountId;
            TransactionId = transactionId;
            TransactionType = transactionType;
            PreparationType = preparationType;
            Reason = reason;
        }
    }
}
