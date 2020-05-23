using System.Linq;
using System.Threading.Tasks;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Messaging;

namespace BankTransferSample.ProcessManagers
{
    /// <summary>
    /// 银行存款交易流程管理器，用于协调银行存款交易流程中各个参与者聚合根之间的消息交互。
    /// </summary>
    public class DepositTransactionProcessManager :
        IMessageHandler<DepositTransactionStartedEvent>,                    //存款交易已开始
        IMessageHandler<DepositTransactionPreCommitSucceedEvent>,                  //账户预操作已添加
        IMessageHandler<DepositTransactionAllParticipantPreCommitSucceedEvent>,   //存款交易已提交     
        IMessageHandler<DepositTransactionCommittedEvent>               //账户预操作已提交
    {
        private ICommandService _commandService;

        public DepositTransactionProcessManager(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public async Task HandleAsync(DepositTransactionStartedEvent evnt)
        {
            await _commandService.SendAsync(new PreCommitDepositTransactionPreparationCommand
            {
                Id = evnt.Id,
                Items = evnt.Items,
                AggregateRootId = evnt.AccountId,
                TransactionId = evnt.TransactionId,
                TransactionType = evnt.TransactionType,
                InitiatorId = evnt.AggregateRootId,
                InitiatorType = (byte)AggregateRootTypes.DepositTransaction,
                Amount = evnt.Amount
            });
        }

        public async Task HandleAsync(DepositTransactionPreCommitSucceedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.DepositTransaction)
            {
                await _commandService.SendAsync(new AddDepositPreCommitSuccessParticipantCommand
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
        
        public async Task HandleAsync(DepositTransactionAllParticipantPreCommitSucceedEvent evnt)
        {
            await _commandService.SendAsync(new CommitTransactionPreparationCommand(evnt.TransactionParticipants.First().ParticipantId, evnt.AggregateRootId) { Id = evnt.Id, Items = evnt.Items });
        }

        public async Task HandleAsync(DepositTransactionCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == (byte)TransactionTypes.DepositTransaction)
            {
                await _commandService.SendAsync(new AddDepositCommittedParticipantCommand
                {
                    Id = evnt.Id,
                    Items = evnt.Items,
                    AggregateRootId = evnt.TransactionPreparation.InitiatorId,
                    TransactionId = evnt.TransactionPreparation.TransactionId,
                    TransactionType = evnt.TransactionPreparation.TransactionType,
                    ParticipantId = evnt.AggregateRootId,
                    ParticipantType = evnt.TransactionPreparation.ParticipantType
                });
            }
        }
    }
}
