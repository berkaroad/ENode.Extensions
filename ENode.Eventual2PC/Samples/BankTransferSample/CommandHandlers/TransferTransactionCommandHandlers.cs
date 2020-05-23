using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using System.Threading.Tasks;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>
    /// 银行转账交易相关命令处理
    /// </summary>
    public class TransferTransactionCommandHandlers :
        ICommandHandler<StartTransferTransactionCommand>,                       //开始转账交易
        ICommandHandler<AddTransferPreCommitSuccessParticipantCommand>,
        ICommandHandler<AddTransferPreCommitFailParticipantCommand>,
        ICommandHandler<AddTransferCommittedParticipantCommand>,
        ICommandHandler<AddTransferRolledbackParticipantCommand>
    {
        public Task HandleAsync(ICommandContext context, StartTransferTransactionCommand command)
        {
            return context.AddAsync(new TransferTransaction(command.AggregateRootId, command.TransactionInfo));
        }

        public async Task HandleAsync(ICommandContext context, AddTransferPreCommitSuccessParticipantCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.AddPreCommitSuccessParticipant(command.AggregateRootId, command.TransactionType, new Eventual2PC.TransactionParticipantInfo(command.ParticipantId, command.ParticipantType));
        }

        public async Task HandleAsync(ICommandContext context, AddTransferPreCommitFailParticipantCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.AddPreCommitFailedParticipant(command.AggregateRootId, command.TransactionType, new Eventual2PC.TransactionParticipantInfo(command.ParticipantId, command.ParticipantType));
        }

        public async Task HandleAsync(ICommandContext context, AddTransferCommittedParticipantCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.AddCommittedParticipant(command.AggregateRootId, command.TransactionType, new Eventual2PC.TransactionParticipantInfo(command.ParticipantId, command.ParticipantType));
        }

        public async Task HandleAsync(ICommandContext context, AddTransferRolledbackParticipantCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.AddRolledbackParticipant(command.AggregateRootId, command.TransactionType, new Eventual2PC.TransactionParticipantInfo(command.ParticipantId, command.ParticipantType));
        }
    }
}
