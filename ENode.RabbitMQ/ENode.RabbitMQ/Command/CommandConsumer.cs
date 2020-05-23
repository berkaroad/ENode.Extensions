using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using RabbitMQTopic;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// Command Consumer
    /// </summary>
    public class CommandConsumer
    {
        private SendReplyService _sendReplyService;
        private IJsonSerializer _jsonSerializer;
        private ITypeNameProvider _typeNameProvider;
        private ICommandProcessor _commandProcessor;
        private IRepository _repository;
        private IAggregateStorage _aggregateStorage;
        private ILogger _logger;
        private Consumer _consumer;

        /// <summary>
        /// Initialize ENode
        /// </summary>
        /// <returns></returns>
        public CommandConsumer InitializeENode()
        {
            _sendReplyService = new SendReplyService("CommandConsumerSendReplyService");
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _commandProcessor = ObjectContainer.Resolve<ICommandProcessor>();
            _repository = ObjectContainer.Resolve<IRepository>();
            _aggregateStorage = ObjectContainer.Resolve<IAggregateStorage>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            return this;
        }

        /// <summary>
        /// Initialize RabbitMQ
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="autoConfig"></param>
        /// <returns></returns>
        public CommandConsumer InitializeRabbitMQ(ConsumerSettings settings, bool autoConfig = true)
        {
            InitializeENode();
            _consumer = new Consumer(settings, autoConfig);
            return this;
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <returns></returns>
        public CommandConsumer Start()
        {
            _sendReplyService.Start();
            _consumer.OnMessageReceived += (sender, e) =>
            {
                try
                {
                    var commandItems = new Dictionary<string, string>();
                    var commandMessage = _jsonSerializer.Deserialize<CommandMessage>(Encoding.UTF8.GetString(e.Context.GetBody()));
                    var commandType = _typeNameProvider.GetType(e.Context.GetMessageType());
                    var command = _jsonSerializer.Deserialize(commandMessage.CommandData, commandType) as ICommand;
                    var commandExecuteContext = new CommandExecuteContext(_repository, _aggregateStorage, commandMessage, _sendReplyService, e.Context, _logger);
                    commandItems["CommandReplyAddress"] = commandMessage.ReplyAddress;
                    _logger.DebugFormat("ENode command message received, messageId: {0}, aggregateRootId: {1}", command.Id, command.AggregateRootId);
                    _commandProcessor.Process(new ProcessingCommand(command, commandExecuteContext, commandItems));
                }
                catch (Exception ex)
                {
                    _logger.Error($"ENode command message handle failed: {ex.Message}, commandMessage: {e.Context.GetBody()}, commandType: {e.Context.GetMessageType()}", ex);
                }
            };
            _consumer.Start();
            return this;
        }

        /// <summary>
        /// Subscribe topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="queueCount"></param>
        /// <returns></returns>
        public CommandConsumer Subscribe(string topic, int queueCount = 4)
        {
            _consumer.Subscribe(topic, queueCount);
            return this;
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        /// <returns></returns>
        public CommandConsumer Shutdown()
        {
            _consumer.Shutdown();
            _sendReplyService.Stop();
            return this;
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private IApplicationMessage _applicationMessage;
            private string _result;
            private readonly ConcurrentDictionary<string, IAggregateRoot> _trackingAggregateRootDict;
            private readonly IRepository _repository;
            private readonly IAggregateStorage _aggregateRootStorage;
            private readonly SendReplyService _sendReplyService;
            private readonly IMessageTransportationContext _messageContext;
            private readonly CommandMessage _commandMessage;
            private ILogger _logger;

            public CommandExecuteContext(IRepository repository, IAggregateStorage aggregateRootStorage, CommandMessage commandMessage, SendReplyService sendReplyService, IMessageTransportationContext messageContext, ILogger logger)
            {
                _trackingAggregateRootDict = new ConcurrentDictionary<string, IAggregateRoot>();
                _repository = repository;
                _aggregateRootStorage = aggregateRootStorage;
                _sendReplyService = sendReplyService;
                _messageContext = messageContext;
                _commandMessage = commandMessage;
                _logger = logger;
            }

            public Task OnCommandExecutedAsync(CommandResult commandResult)
            {
                if (!string.IsNullOrEmpty(_commandMessage.SagaId))
                {
                    if (commandResult.Status != CommandStatus.Failed
                        || (!string.IsNullOrEmpty(commandResult.ResultType) && commandResult.ResultType != "System.String" && commandResult.ResultType.EndsWith("DomainException")))
                    {
                        _messageContext.Ack();
                    }
                    else
                    {
                        _logger.Error($"ENode command executed failed: [{commandResult.ResultType}]{commandResult.Result}, commandData: {_commandMessage.CommandData}, commandType: {_messageContext.GetMessageType()}, sagaId: {_commandMessage.SagaId}");
                    }
                    return Task.CompletedTask;
                }

                _messageContext.Ack();
                if (!string.IsNullOrEmpty(_commandMessage.ReplyAddress))
                {
                    return _sendReplyService.SendReply((int)CommandReturnType.CommandExecuted, commandResult, _commandMessage.ReplyAddress);
                }
                return Task.CompletedTask;
            }
            public void Add(IAggregateRoot aggregateRoot)
            {
                if (aggregateRoot == null)
                {
                    throw new ArgumentNullException("aggregateRoot");
                }
                if (!_trackingAggregateRootDict.TryAdd(aggregateRoot.UniqueId, aggregateRoot))
                {
                    throw new AggregateRootAlreadyExistException(aggregateRoot.UniqueId, aggregateRoot.GetType());
                }
            }
            public Task AddAsync(IAggregateRoot aggregateRoot)
            {
                Add(aggregateRoot);
                return Task.CompletedTask;
            }
            public async Task<T> GetAsync<T>(object id, bool firstFromCache = true) where T : class, IAggregateRoot
            {
                if (id == null)
                {
                    throw new ArgumentNullException("id");
                }

                var aggregateRootId = id.ToString();
                if (_trackingAggregateRootDict.TryGetValue(aggregateRootId, out IAggregateRoot aggregateRoot))
                {
                    return aggregateRoot as T;
                }

                if (firstFromCache)
                {
                    aggregateRoot = await _repository.GetAsync<T>(id).ConfigureAwait(false);
                }
                else
                {
                    aggregateRoot = await _aggregateRootStorage.GetAsync(typeof(T), aggregateRootId).ConfigureAwait(false);
                }

                if (aggregateRoot != null)
                {
                    _trackingAggregateRootDict.TryAdd(aggregateRoot.UniqueId, aggregateRoot);
                    return aggregateRoot as T;
                }

                return null;
            }
            public IEnumerable<IAggregateRoot> GetTrackedAggregateRoots()
            {
                return _trackingAggregateRootDict.Values;
            }
            public void Clear()
            {
                _trackingAggregateRootDict.Clear();
                _result = null;
            }
            public void SetResult(string result)
            {
                _result = result;
            }
            public string GetResult()
            {
                return _result;
            }

            public void SetApplicationMessage(IApplicationMessage applicationMessage)
            {
                _applicationMessage = applicationMessage;
            }

            public IApplicationMessage GetApplicationMessage()
            {
                return _applicationMessage;
            }
        }
    }
}
