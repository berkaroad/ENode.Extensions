using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using BankTransferSample.EventHandlers;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Serilog;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;

namespace BankTransferSample
{
    class Program
    {
        static ENodeConfiguration _configuration;

        static void Main(string[] args)
        {
            NormalTest();
        }

        static void NormalTest()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            var loggerFactory = new SerilogLoggerFactory()
                .AddFileLogger("ECommon", Path.Combine("logs", "ecommon"))
                .AddFileLogger("ENode", Path.Combine("logs", "enode"))
                .AddFileLogger("ENode.RabbitMQ", Path.Combine("logs", "enode.rabbitmq"));
            _configuration = ECommon.Configurations.Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseSerilog(loggerFactory)
                .UseJsonNet()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .UseRabbitMQ()
                .BuildContainer()
                .InitializeBusinessAssemblies(assemblies)
                .StartRabbitMQ()
                .Start();

            Console.WriteLine(string.Empty);
            Console.WriteLine("ENode started...");

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var syncHelper = ObjectContainer.Resolve<SyncHelper>();
            var account1 = ObjectId.GenerateNewStringId();
            var account2 = ObjectId.GenerateNewStringId();
            var account3 = "INVALID-" + ObjectId.GenerateNewStringId();
            Console.WriteLine(string.Empty);

            //创建两个银行账户
            commandService.ExecuteAsync(new CreateAccountCommand(account1, "雪华"), CommandReturnType.EventHandled).Wait();
            commandService.ExecuteAsync(new CreateAccountCommand(account2, "凯锋"), CommandReturnType.EventHandled).Wait();

            Console.WriteLine(string.Empty);

            //每个账户都存入1000元
            commandService.SendAsync(new StartDepositTransactionCommand(ObjectId.GenerateNewStringId(), account1, 1000)).Wait();
            syncHelper.WaitOne();
            commandService.SendAsync(new StartDepositTransactionCommand(ObjectId.GenerateNewStringId(), account2, 1000)).Wait();
            syncHelper.WaitOne();

            Console.WriteLine(string.Empty);

            //账户1向账户3转账300元，交易会失败，因为账户3不存在
            commandService.SendAsync(new StartTransferTransactionCommand(ObjectId.GenerateNewStringId(), new TransferTransactionInfo(account1, account3, 300D))
            {
                Items = new Dictionary<string, string>
                {
                    { "ProcessId", "10000" }
                }
            }).Wait();
            syncHelper.WaitOne();
            Console.WriteLine(string.Empty);

            //账户1向账户2转账1200元，交易会失败，因为余额不足
            commandService.SendAsync(new StartTransferTransactionCommand(ObjectId.GenerateNewStringId(), new TransferTransactionInfo(account1, account2, 1200D))
            {
                Items = new Dictionary<string, string>
                {
                    { "ProcessId", "10001" }
                }
            }).Wait();
            syncHelper.WaitOne();
            Console.WriteLine(string.Empty);

            //账户2向账户1转账500元，交易成功
            commandService.SendAsync(new StartTransferTransactionCommand(ObjectId.GenerateNewStringId(), new TransferTransactionInfo(account2, account1, 500D))
            {
                Items = new Dictionary<string, string>
                {
                    { "ProcessId", "10002" }
                }
            }).Wait();
            syncHelper.WaitOne();

            Thread.Sleep(10000);
            _configuration.ShutdownRabbitMQ().Stop();
            Console.WriteLine("Simple test complete.");
        }
    }
}
