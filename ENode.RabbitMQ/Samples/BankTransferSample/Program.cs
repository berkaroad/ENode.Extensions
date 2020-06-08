﻿using System;
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
            var mode = ConfigurationManager.AppSettings["mode"];
            if (mode == "simple")
            {
                NormalTest();
            }
            else if (mode == "concurrent")
            {
                PerformanceTest();
            }
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
        static void PerformanceTest()
        {
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly()
            };
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
                .RegisterUnhandledExceptionHandler()
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
            var countSyncHelper = ObjectContainer.Resolve<CountSyncHelper>();

            Console.WriteLine(string.Empty);

            var accountList = new List<string>();
            var accountCount = 100;
            var transactionCount = 1000;
            var depositAmount = 1000000000D;
            var transferAmount = 100D;

            //创建银行账户
            for (var i = 0; i < accountCount; i++)
            {
                var accountId = ObjectId.GenerateNewStringId();
                commandService.ExecuteAsync(new CreateAccountCommand(accountId, "SampleAccount" + i), CommandReturnType.EventHandled).Wait();
                accountList.Add(accountId);
            }

            Console.WriteLine(string.Empty);

            //每个账户都存入初始额度
            foreach (var accountId in accountList)
            {
                commandService.SendAsync(new StartDepositTransactionCommand(ObjectId.GenerateNewStringId(), accountId, depositAmount)).Wait();
                syncHelper.WaitOne();
            }

            Console.WriteLine(string.Empty);

            countSyncHelper.SetExpectedCount(transactionCount);

            var watch = Stopwatch.StartNew();
            for (var i = 0; i < transactionCount; i++)
            {
                var sourceAccountIndex = new Random().Next(accountCount - 1);
                var targetAccountIndex = sourceAccountIndex + 1;
                var sourceAccount = accountList[sourceAccountIndex];
                var targetAccount = accountList[targetAccountIndex];
                commandService.SendAsync(new StartTransferTransactionCommand(ObjectId.GenerateNewStringId(), new TransferTransactionInfo(sourceAccount, targetAccount, transferAmount)));
            }

            countSyncHelper.WaitOne();

            var spentTime = watch.ElapsedMilliseconds;
            Thread.Sleep(500);
            Console.WriteLine(string.Empty);
            Console.WriteLine("All transfer transactions completed, time spent: {0}ms, throughput: {1} transactions per second.", spentTime, transactionCount * 1000 / spentTime);

            Thread.Sleep(10000);
            _configuration.ShutdownRabbitMQ().Stop();
            Console.WriteLine("Concurrent test complete.");

        }
    }
}
