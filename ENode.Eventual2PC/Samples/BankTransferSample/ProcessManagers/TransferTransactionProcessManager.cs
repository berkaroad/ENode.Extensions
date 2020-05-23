using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankTransferSample.ApplicationMessages;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Messaging;

namespace BankTransferSample.ProcessManagers
{
    /// <summary>
    /// 银行转账交易流程管理器，用于协调银行转账交易流程中各个参与者聚合根之间的消息交互。
    /// </summary>
    public class TransferTransactionProcessManager :
        IMessageHandler<TransferTransactionStartedEvent>,                  //转账交易已开始
        IMessageHandler<AccountValidatePassedMessage>,                     //账户验证已通过
        IMessageHandler<AccountValidateFailedMessage>,                     //账户验证未通过
        IMessageHandler<WithdrawTransactionPreCommitSucceedEvent>,      // 账户预支出操作已添加
        IMessageHandler<DepositTransactionPreCommitSucceedEvent>,                 //账户预支入操作已添加
        IMessageHandler<InsufficientBalanceDomainException>,                     //账户余额不足
        IMessageHandler<TransferTransactionAllParticipantPreCommitSucceedEvent>,          //转账交易预转出已确认
        IMessageHandler<TransferTransactionAnyParticipantPreCommitFailedEvent>, 
        IMessageHandler<WithdrawTransactionCommittedEvent>,              //账户预操作已提交
        IMessageHandler<WithdrawTransactionRolledbackEvent>,              //账户预操作已回滚
        IMessageHandler<DepositTransactionCommittedEvent>,               //账户预操作已提交
        IMessageHandler<DepositTransactionRolledbackEvent>               //账户预操作已回滚
    {
        private ICommandService _commandService;

        public TransferTransactionProcessManager(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public async Task HandleAsync(TransferTransactionStartedEvent evnt)
        {
            var task1 = _commandService.SendAsync(new ValidateAccountCommand(
                evnt.TransactionInfo.SourceAccountId,
                evnt.AggregateRootId,
                (byte)TransactionTypes.TransferTransaction,
                PreparationTypes.DebitPreparation, evnt.TransactionInfo.Amount)
            { Id = evnt.Id, Items = evnt.Items });
            var task2 = _commandService.SendAsync(new ValidateAccountCommand(
                evnt.TransactionInfo.TargetAccountId,
                evnt.AggregateRootId,
                (byte)TransactionTypes.TransferTransaction,
                PreparationTypes.CreditPreparation, evnt.TransactionInfo.Amount)
            { Id = evnt.Id, Items = evnt.Items });
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
        }
        
        public async Task HandleAsync(AccountValidatePassedMessage message)
        {
            if (message.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                if (message.PreparationType == PreparationTypes.DebitPreparation)
                {
                    await _commandService.SendAsync(new PreCommitWithdrawTransactionPreparationCommand
                    {
                        Id = message.Id,
                        Items = message.Items,
                        AggregateRootId = message.AccountId,
                        TransactionId = message.TransactionId,
                        TransactionType = message.TransactionType,
                        InitiatorId = message.TransactionId,
                        InitiatorType = (byte)AggregateRootTypes.TransferTransaction,
                        Amount = message.Amount
                    });
                }
                else if (message.PreparationType == PreparationTypes.CreditPreparation)
                {
                    await _commandService.SendAsync(new PreCommitDepositTransactionPreparationCommand
                    {
                        Id = message.Id,
                        Items = message.Items,
                        AggregateRootId = message.AccountId,
                        TransactionId = message.TransactionId,
                        TransactionType = message.TransactionType,
                        InitiatorId = message.TransactionId,
                        InitiatorType = (byte)AggregateRootTypes.TransferTransaction,
                        Amount = message.Amount
                    });
                }
            }
        }

        public async Task HandleAsync(AccountValidateFailedMessage message)
        {
            if (message.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                await _commandService.SendAsync(new AddTransferPreCommitFailParticipantCommand
                {
                    Id = message.Id,
                    Items = message.Items,
                    AggregateRootId = message.TransactionId,
                    TransactionId = message.TransactionId,
                    TransactionType = message.TransactionType,
                    ParticipantId = message.AccountId,
                    ParticipantType = (byte)AggregateRootTypes.BankAccount
                });
            }
        }

        public async Task HandleAsync(WithdrawTransactionPreCommitSucceedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                await _commandService.SendAsync(new AddTransferPreCommitSuccessParticipantCommand
                {
                    Id = evnt.Id,
                    Items = evnt.Items,
                    AggregateRootId = evnt.TransactionPreparation.InitiatorId,
                    TransactionId = evnt.TransactionPreparation.TransactionId,
                    TransactionType = evnt.TransactionPreparation.TransactionType,
                    ParticipantId = evnt.AggregateRootId,
                    ParticipantType = (byte)AggregateRootTypes.BankAccount
                });
            }
        }

        public async Task HandleAsync(DepositTransactionPreCommitSucceedEvent message)
        {
            if (message.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                await _commandService.SendAsync(new AddTransferPreCommitSuccessParticipantCommand
                {
                    Id = message.Id,
                    Items = message.Items,
                    AggregateRootId = message.TransactionPreparation.InitiatorId,
                    TransactionId = message.TransactionPreparation.TransactionId,
                    TransactionType = message.TransactionPreparation.TransactionType,
                    ParticipantId = message.AggregateRootId,
                    ParticipantType = (byte)AggregateRootTypes.BankAccount
                });
            }
        }

        public async Task HandleAsync(InsufficientBalanceDomainException exception)
        {
            if (exception.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                await _commandService.SendAsync(new AddTransferPreCommitFailParticipantCommand
                {
                    Id = exception.Id,
                    Items = exception.Items,
                    AggregateRootId = exception.TransactionPreparation.TransactionId,
                    TransactionId = exception.TransactionPreparation.TransactionId,
                    TransactionType = exception.TransactionPreparation.TransactionType,
                    ParticipantId = exception.TransactionPreparation.ParticipantId,
                    ParticipantType = exception.TransactionPreparation.ParticipantType,
                });
            }
        }

        public async Task HandleAsync(TransferTransactionAllParticipantPreCommitSucceedEvent message)
        {
            var taskList = new List<Task>();
            foreach(var participant in message.TransactionParticipants)
            {
                taskList.Add(_commandService.SendAsync(new CommitTransactionPreparationCommand(participant.ParticipantId, message.TransactionId)
                {
                    Id = message.Id,
                    Items = message.Items
                }));
            }
            await Task.WhenAll(taskList);
        }

        public async Task HandleAsync(TransferTransactionAnyParticipantPreCommitFailedEvent message)
        {
            var taskList = new List<Task>();
            foreach (var participant in message.PreCommitSucceedTransactionParticipants)
            {
                taskList.Add(_commandService.SendAsync(new RollbackTransactionPreparationCommand(participant.ParticipantId, message.TransactionId)
                {
                    Id = message.Id,
                    Items = message.Items
                }));
            }
            await Task.WhenAll(taskList);
        }

        public async Task HandleAsync(WithdrawTransactionCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                await _commandService.SendAsync(new AddTransferCommittedParticipantCommand
                {
                    Id = evnt.Id,
                    Items = evnt.Items,
                    AggregateRootId = evnt.TransactionPreparation.InitiatorId,
                    TransactionId = evnt.TransactionPreparation.TransactionId,
                    TransactionType = evnt.TransactionPreparation.TransactionType,
                    ParticipantId = evnt.TransactionPreparation.ParticipantId,
                    ParticipantType = evnt.TransactionPreparation.ParticipantType
                });
            }
        }

        public async Task HandleAsync(WithdrawTransactionRolledbackEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                await _commandService.SendAsync(new AddTransferRolledbackParticipantCommand
                {
                    Id = evnt.Id,
                    Items = evnt.Items,
                    AggregateRootId = evnt.TransactionPreparation.InitiatorId,
                    TransactionId = evnt.TransactionPreparation.TransactionId,
                    TransactionType = evnt.TransactionPreparation.TransactionType,
                    ParticipantId = evnt.TransactionPreparation.ParticipantId,
                    ParticipantType = evnt.TransactionPreparation.ParticipantType
                });
            }
        }

        public async Task HandleAsync(DepositTransactionCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                await _commandService.SendAsync(new AddTransferCommittedParticipantCommand
                {
                    Id = evnt.Id,
                    Items = evnt.Items,
                    AggregateRootId = evnt.TransactionPreparation.InitiatorId,
                    TransactionId = evnt.TransactionPreparation.TransactionId,
                    TransactionType = evnt.TransactionPreparation.TransactionType,
                    ParticipantId = evnt.TransactionPreparation.ParticipantId,
                    ParticipantType = evnt.TransactionPreparation.ParticipantType
                });
            }
        }

        public async Task HandleAsync(DepositTransactionRolledbackEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.TransferTransaction)
            {
                await _commandService.SendAsync(new AddTransferRolledbackParticipantCommand
                {
                    Id = evnt.Id,
                    Items = evnt.Items,
                    AggregateRootId = evnt.TransactionPreparation.InitiatorId,
                    TransactionId = evnt.TransactionPreparation.TransactionId,
                    TransactionType = evnt.TransactionPreparation.TransactionType,
                    ParticipantId = evnt.TransactionPreparation.ParticipantId,
                    ParticipantType = evnt.TransactionPreparation.ParticipantType
                });
            }
        }
    }
}
