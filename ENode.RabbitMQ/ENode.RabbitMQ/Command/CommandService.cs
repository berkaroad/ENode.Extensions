using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Infrastructure;
using RabbitMQTopic;
using TopicMessage = RabbitMQTopic.Message;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// Command service
    /// </summary>
    public class CommandService : ICommandService
    {
        private ILogger _logger;
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<ICommand> _commandTopicProvider;
        private ITypeNameProvider _typeNameProvider;
        private SendQueueMessageService _sendMessageService;
        private CommandResultProcessor _commandResultProcessor;
        private string _sagaIdCommandItemKey;
        private IOHelper _ioHelper;
        private Producer _producer;

        /// <summary>
        /// Initialize ENode
        /// </summary>
        /// <returns></returns>
        public CommandService InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _commandTopicProvider = ObjectContainer.Resolve<ITopicProvider<ICommand>>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _sendMessageService = new SendQueueMessageService();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            return this;
        }

        /// <summary>
        /// Initialize RabbitMQ
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="topic"></param>
        /// <param name="queueCount"></param>
        /// <param name="delayedCommandEnabled">Enable delayed command or not. If enable, please make sure that plugin 'rabbitmq_delayed_message_exchange' enabled.</param>
        /// <param name="autoConfig"></param>
        /// <param name="commandResultProcessor"></param>
        /// <param name="sagaIdCommandItemKey">Command items's key for store saga id</param>
        /// <returns></returns>
        public CommandService InitializeRabbitMQ(ProducerSettings settings, string topic, int queueCount = 4, bool delayedCommandEnabled = false, bool autoConfig = true, CommandResultProcessor commandResultProcessor = null, string sagaIdCommandItemKey = "SagaId")
        {
            InitializeENode();
            _producer = new Producer(settings, delayedCommandEnabled, autoConfig);
            _producer.RegisterTopic(topic, queueCount);
            _commandResultProcessor = commandResultProcessor;
            _sagaIdCommandItemKey = sagaIdCommandItemKey;
            return this;
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <returns></returns>
        public CommandService Start()
        {
            if (_commandResultProcessor != null)
            {
                _commandResultProcessor.Start();
            }
            _producer.Start();
            return this;
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        /// <returns></returns>
        public CommandService Shutdown()
        {
            _producer.Shutdown();
            if (_commandResultProcessor != null)
            {
                _commandResultProcessor.Shutdown();
            }
            return this;
        }

        /// <summary>
        /// Send command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Task SendAsync(ICommand command)
        {
            return _sendMessageService.SendMessageAsync(_producer, "command", command.GetType().Name, BuildCommandMessage(command, false, _sagaIdCommandItemKey), command.AggregateRootId, command.Id, command.Items);
        }

        /// <summary>
        /// Execute command and wait for command executed.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Task<CommandResult> ExecuteAsync(ICommand command)
        {
            return ExecuteAsync(command, CommandReturnType.CommandExecuted);
        }

        /// <summary>
        /// Execute command and wait for specific command return type.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandReturnType"></param>
        /// <returns></returns>
        public async Task<CommandResult> ExecuteAsync(ICommand command, CommandReturnType commandReturnType)
        {
            Ensure.NotNull(_commandResultProcessor, "commandResultProcessor");
            var taskCompletionSource = new TaskCompletionSource<CommandResult>();
            _commandResultProcessor.RegisterProcessingCommand(command, commandReturnType, taskCompletionSource);

            try
            {
                await _sendMessageService.SendMessageAsync(_producer, "command", command.GetType().Name, BuildCommandMessage(command, true, _sagaIdCommandItemKey), command.AggregateRootId, command.Id, command.Items).ConfigureAwait(false);
            }
            catch
            {
                _commandResultProcessor.ProcessFailedSendingCommand(command);
                throw;
            }

            return await taskCompletionSource.Task.ConfigureAwait(false);
        }

        private TopicMessage BuildCommandMessage(ICommand command, bool needReply, string sagaIdCommandItemKey)
        {
            var delayedCommand = command as DelayedCommand;
            var realCommand = delayedCommand == null ? command : delayedCommand.GetWrappedCommand();
            Ensure.NotNull(realCommand.AggregateRootId, "aggregateRootId");
            var commandData = _jsonSerializer.Serialize(realCommand);
            var topic = _commandTopicProvider.GetTopic(realCommand);
            var replyAddress = needReply && _commandResultProcessor != null ? _commandResultProcessor.BindingAddress.ToString() : null;
            var sagaId = realCommand.Items != null && realCommand.Items.ContainsKey(sagaIdCommandItemKey) ? realCommand.Items[sagaIdCommandItemKey] : null;
            var messageData = _jsonSerializer.Serialize(new CommandMessage
            {
                CommandData = commandData,
                ReplyAddress = replyAddress,
                SagaId = string.IsNullOrEmpty(sagaId) ? null : sagaId
            });
            return new TopicMessage(
                topic,
                (int)MessageTypeCode.CommandMessage,
                Encoding.UTF8.GetBytes(messageData),
                "text/json",
                delayedCommand == null ? 0 : delayedCommand.DelayedMilliseconds,
                _typeNameProvider.GetTypeName(realCommand.GetType()));
        }
    }
}
