﻿using System;
using System.Threading.Tasks;
using BankTransferSample2.ApplicationMessages;
using BankTransferSample2.Domain;
using ECommon.IO;
using ENode.Messaging;

namespace BankTransferSample2.EventHandlers
{
    public class ConsoleLogger :
        IMessageHandler<AccountCreatedEvent>,
        IMessageHandler<AccountValidatePassedMessage>,
        IMessageHandler<AccountValidateFailedMessage>,
        IMessageHandler<TransactionPreparationAddedEvent>,
        IMessageHandler<TransactionPreparationCommittedEvent>,
        IMessageHandler<TransferTransactionStartedEvent>,
        IMessageHandler<TransferOutPreparationConfirmedEvent>,
        IMessageHandler<TransferInPreparationConfirmedEvent>,
        IMessageHandler<TransferTransactionCompletedEvent>,
        IMessageHandler<InsufficientBalanceException>,
        IMessageHandler<TransferTransactionCanceledEvent>
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
        public Task HandleAsync(TransactionPreparationAddedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    Console.WriteLine("账户预转出成功，交易ID：{0}，账户：{1}，金额：{2}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount);
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    Console.WriteLine("账户预转入成功，交易ID：{0}，账户：{1}，金额：{2}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount);
                }
            }
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransactionPreparationCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.DepositTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    Console.WriteLine("账户存款已成功，账户：{0}，金额：{1}，当前余额：{2}", evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
            }
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    Console.WriteLine("账户转出已成功，交易ID：{0}，账户：{1}，金额：{2}，当前余额：{3}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
                if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    Console.WriteLine("账户转入已成功，交易ID：{0}，账户：{1}，金额：{2}，当前余额：{3}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(TransferTransactionStartedEvent evnt)
        {
            Console.WriteLine("转账交易已开始，交易ID：{0}，源账户：{1}，目标账户：{2}，转账金额：{3}", evnt.AggregateRootId, evnt.TransactionInfo.SourceAccountId, evnt.TransactionInfo.TargetAccountId, evnt.TransactionInfo.Amount);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferOutPreparationConfirmedEvent evnt)
        {
            Console.WriteLine("预转出确认成功，交易ID：{0}，账户：{1}", evnt.AggregateRootId, evnt.TransactionInfo.SourceAccountId);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferInPreparationConfirmedEvent evnt)
        {
            Console.WriteLine("预转入确认成功，交易ID：{0}，账户：{1}", evnt.AggregateRootId, evnt.TransactionInfo.TargetAccountId);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCompletedEvent evnt)
        {
            Console.WriteLine("转账交易已完成，交易ID：{0}", evnt.AggregateRootId);
            return Task.CompletedTask;
        }

        public Task HandleAsync(InsufficientBalanceException exception)
        {
            Console.WriteLine("账户的余额不足，交易ID：{0}，账户：{1}，可用余额：{2}，转出金额：{3}", exception.TransactionId, exception.AccountId, exception.CurrentAvailableBalance, exception.Amount);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCanceledEvent evnt)
        {
            Console.WriteLine("转账交易已取消，交易ID：{0}", evnt.AggregateRootId);
            return Task.CompletedTask;
        }
    }
}
