using System;
using System.Threading.Tasks;
using BankTransferSample.ApplicationMessages;
using BankTransferSample.Domain;
using ENode.Messaging;

namespace BankTransferSample.EventHandlers
{
    public class ConsoleLogger :
        IMessageHandler<AccountCreatedEvent>,
        IMessageHandler<AccountValidatePassedMessage>,
        IMessageHandler<AccountValidateFailedMessage>,
        IMessageHandler<WithdrawTransactionPreCommitSucceedEvent>,
        IMessageHandler<DepositTransactionPreCommitSucceedEvent>,
        IMessageHandler<WithdrawTransactionCommittedEvent>,
        IMessageHandler<DepositTransactionCommittedEvent>,
        IMessageHandler<TransferTransactionStartedEvent>,
        IMessageHandler<TransferTransactionPreCommitSucceedParticipantAdded>,
        IMessageHandler<WithdrawTransactionRolledbackEvent>,
        IMessageHandler<DepositTransactionRolledbackEvent>,
        IMessageHandler<TransferTransactionCompletedEvent>,
        IMessageHandler<InsufficientBalanceDomainException>
    {
        public Task HandleAsync(AccountCreatedEvent evnt)
        {
            Console.WriteLine("账户已创建，账户：{0}，所有者：{1}", evnt.AggregateRootId, evnt.Owner);
            return Task.CompletedTask;
        }
        public Task HandleAsync(AccountValidatePassedMessage message)
        {
            Console.WriteLine("账户验证已通过，交易ID：{0}，账户：{1}", message.TransactionId, message.AccountId);
            return Task.CompletedTask;
        }
        public Task HandleAsync(AccountValidateFailedMessage message)
        {
            Console.WriteLine("无效的银行账户，交易ID：{0}，账户：{1}，理由：{2}", message.TransactionId, message.AccountId, message.Reason);
            return Task.CompletedTask;
        }
        public Task HandleAsync(WithdrawTransactionPreCommitSucceedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                Console.WriteLine("账户预转出成功，交易ID：{0}，账户：{1}，金额：{2}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.ParticipantId, evnt.TransactionPreparation.Amount);
            }
            return Task.CompletedTask;
        }
        public Task HandleAsync(DepositTransactionPreCommitSucceedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                Console.WriteLine("账户预转入成功，交易ID：{0}，账户：{1}，金额：{2}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.ParticipantId, evnt.TransactionPreparation.Amount);
            }
            return Task.CompletedTask;
        }
        public Task HandleAsync(WithdrawTransactionCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                Console.WriteLine("账户转出已成功，交易ID：{0}，账户：{1}，金额：{2}，当前余额：{3}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.ParticipantId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
            }
            return Task.CompletedTask;
        }
        public Task HandleAsync(DepositTransactionCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.DepositTransaction)
            {
                Console.WriteLine("账户存款已成功，账户：{0}，金额：{1}，当前余额：{2}", evnt.TransactionPreparation.ParticipantId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
            }
            else if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                Console.WriteLine("账户转入已成功，交易ID：{0}，账户：{1}，金额：{2}，当前余额：{3}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.ParticipantId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(TransferTransactionStartedEvent evnt)
        {
            Console.WriteLine("转账交易已开始，交易ID：{0}，源账户：{1}，目标账户：{2}，转账金额：{3}", evnt.AggregateRootId, evnt.TransactionInfo.SourceAccountId, evnt.TransactionInfo.TargetAccountId, evnt.TransactionInfo.Amount);
            return Task.CompletedTask;
        }

        public Task HandleAsync(TransferTransactionPreCommitSucceedParticipantAdded evnt)
        {
            if (evnt.PreparationType == PreparationTypes.DebitPreparation)
            {
                Console.WriteLine("预转出确认成功，交易ID：{0}，账户：{1}", evnt.AggregateRootId, evnt.TransactionParticipant.ParticipantId);
            }
            else
            {
                Console.WriteLine("预转入确认成功，交易ID：{0}，账户：{1}", evnt.AggregateRootId, evnt.TransactionParticipant.ParticipantId);
            }
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCompletedEvent evnt)
        {
            if (evnt.IsCommitSuccess)
            {
                Console.WriteLine("转账交易已完成，交易ID：{0}", evnt.AggregateRootId);
            }
            else
            {
                Console.WriteLine("转账交易已取消，交易ID：{0}", evnt.AggregateRootId);
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(InsufficientBalanceDomainException exception)
        {
            Console.WriteLine("账户的余额不足，交易ID：{0}，账户：{1}，可用余额：{2}，转出金额：{3}", exception.TransactionPreparation.TransactionId, exception.TransactionPreparation.ParticipantId, exception.CurrentAvailableBalance, exception.Amount);
            return Task.CompletedTask;
        }

        public Task HandleAsync(WithdrawTransactionRolledbackEvent message)
        {
            Console.WriteLine("账户已回滚预支出，交易ID：{0}，账户：{1}", message.TransactionPreparation.TransactionId, message.TransactionPreparation.ParticipantId);
            return Task.CompletedTask;
        }

        public Task HandleAsync(DepositTransactionRolledbackEvent message)
        {
            Console.WriteLine("账户已回滚预支入，交易ID：{0}，账户：{1}", message.TransactionPreparation.TransactionId, message.TransactionPreparation.ParticipantId);
            return Task.CompletedTask;
        }
    }
}
