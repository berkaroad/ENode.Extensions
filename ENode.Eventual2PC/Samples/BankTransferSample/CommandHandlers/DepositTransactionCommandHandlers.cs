using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using System.Threading.Tasks;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>
    /// 银行存款交易相关命令处理
    /// </summary>
    public class DepositTransactionCommandHandlers :
        ICommandHandler<StartDepositTransactionCommand>,                      //开始交易
        ICommandHandler<AddDepositPreCommitSuccessParticipantCommand>,
        ICommandHandler<AddDepositCommittedParticipantCommand>
    {
        public Task HandleAsync(ICommandContext context, StartDepositTransactionCommand command)
        {
            return context.AddAsync(new DepositTransaction(command.AggregateRootId, command.AccountId, command.Amount));
        }

        public async Task HandleAsync(ICommandContext context, AddDepositPreCommitSuccessParticipantCommand command)
        {
            var transaction = await context.GetAsync<DepositTransaction>(command.AggregateRootId);
            transaction.AddPreCommitSuccessParticipant(command.TransactionId, command.TransactionType, new Eventual2PC.TransactionParticipantInfo(command.ParticipantId,command.ParticipantType));
        }

        public async Task HandleAsync(ICommandContext context, AddDepositCommittedParticipantCommand command)
        {
            var transaction = await context.GetAsync<DepositTransaction>(command.AggregateRootId);
            transaction.AddCommittedParticipant(command.TransactionId, command.TransactionType, new Eventual2PC.TransactionParticipantInfo(command.ParticipantId, command.ParticipantType));
        }
    }
}
