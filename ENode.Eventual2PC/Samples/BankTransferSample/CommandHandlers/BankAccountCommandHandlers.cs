using System.Threading.Tasks;
using BankTransferSample.ApplicationMessages;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Messaging;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>
    /// 银行账户相关命令处理
    /// </summary>
    public class BankAccountCommandHandlers :
        ICommandHandler<CreateAccountCommand>,                       //开户
        ICommandHandler<ValidateAccountCommand>,                     //验证账户是否合法
        ICommandHandler<PreCommitDepositTransactionPreparationCommand>,    //添加预操作
        ICommandHandler<PreCommitWithdrawTransactionPreparationCommand>,   //添加预操作
        ICommandHandler<CommitTransactionPreparationCommand>,         //提交预操作
        ICommandHandler<RollbackTransactionPreparationCommand>
    {
        public bool CheckCommandHandledFirst
        {
            get { return true; }
        }

        public Task HandleAsync(ICommandContext context, CreateAccountCommand command)
        {
            return context.AddAsync(new BankAccount(command.AggregateRootId, command.Owner));
        }

        public Task HandleAsync(ICommandContext context, ValidateAccountCommand command)
        {
            var applicationMessage = default(ApplicationMessage);

            //此处应该会调用外部接口验证账号是否合法，这里仅仅简单通过账号是否以INVALID字符串开头来判断是否合法；根据账号的合法性，返回不同的应用层消息
            if (command.AggregateRootId.StartsWith("INVALID"))
            {
                applicationMessage = new AccountValidateFailedMessage(command.AggregateRootId, command.TransactionId, command.TransactionType, command.PreparationType, "账户不合法.");
            }
            else
            {
                applicationMessage = new AccountValidatePassedMessage(command.AggregateRootId, command.TransactionId, command.TransactionType, command.PreparationType, command.Amount);
            }

            context.SetApplicationMessage(applicationMessage);
            return Task.CompletedTask;
        }
        
        public async Task HandleAsync(ICommandContext context, PreCommitDepositTransactionPreparationCommand command)
        {
            var account = await context.GetAsync<BankAccount>(command.AggregateRootId);
            account.PreCommit(new DepositTransactionPreparation(command.AggregateRootId, command.TransactionId, command.TransactionType, command.Amount));
        }

        public async Task HandleAsync(ICommandContext context, PreCommitWithdrawTransactionPreparationCommand command)
        {
            var account = await context.GetAsync<BankAccount>(command.AggregateRootId);
            account.PreCommit(new WithdrawTransactionPreparation(command.AggregateRootId, command.TransactionId, command.TransactionType, command.Amount));
        }

        public async Task HandleAsync(ICommandContext context, CommitTransactionPreparationCommand command)
        {
            var account = await context.GetAsync<BankAccount>(command.AggregateRootId);
            account.Commit(command.TransactionId);
        }

        public async Task HandleAsync(ICommandContext context, RollbackTransactionPreparationCommand command)
        {
            var account = await context.GetAsync<BankAccount>(command.AggregateRootId);
            account.Rollback(command.TransactionId);
        }
    }
}
