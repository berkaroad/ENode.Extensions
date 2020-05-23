using System;
using System.Runtime.Serialization;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using RabbitMQTopic;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// DomainException Consumer
    /// </summary>
    public class DomainExceptionConsumer
    {
        private IJsonSerializer _jsonSerializer;
        private ITypeNameProvider _typeNameProvider;
        private IMessageDispatcher _messageDispatcher;
        private ILogger _logger;
        private Consumer _consumer;

        /// <summary>
        /// Initialize ENode
        /// </summary>
        /// <returns></returns>
        public DomainExceptionConsumer InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageDispatcher = ObjectContainer.Resolve<IMessageDispatcher>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            return this;
        }

        /// <summary>
        /// Initialize RabbitMQ
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="autoConfig"></param>
        /// <returns></returns>
        public DomainExceptionConsumer InitializeRabbitMQ(ConsumerSettings settings, bool autoConfig = true)
        {
            InitializeENode();
            _consumer = new Consumer(settings, autoConfig);
            return this;
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <returns></returns>
        public DomainExceptionConsumer Start()
        {
            _consumer.OnMessageReceived += (sender, e) =>
            {
                try
                {
                    var exceptionType = _typeNameProvider.GetType(e.Context.GetMessageType());
                    var exceptionMessage = _jsonSerializer.Deserialize<DomainExceptionMessage>(Encoding.UTF8.GetString(e.Context.GetBody()));
                    var exception = FormatterServices.GetUninitializedObject(exceptionType) as IDomainException;
                    exception.Id = exceptionMessage.UniqueId;
                    exception.Timestamp = exceptionMessage.Timestamp;
                    exception.Items = exceptionMessage.Items;
                    exception.RestoreFrom(exceptionMessage.SerializableInfo);
                    _logger.DebugFormat("ENode domain exception message received, messageId: {0}, exceptionType: {1}",
                        exceptionMessage.UniqueId,
                        exceptionType.Name);

                    _messageDispatcher.DispatchMessageAsync(exception).ContinueWith(x =>
                    {
                        e.Context.Ack();
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"ENode domain exception message handle failed: {ex.Message}, exception message: {Encoding.UTF8.GetString(e.Context.GetBody())}", ex);
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
        public DomainExceptionConsumer Subscribe(string topic, int queueCount = 4)
        {
            _consumer.Subscribe(topic, queueCount);
            return this;
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        /// <returns></returns>
        public DomainExceptionConsumer Shutdown()
        {
            _consumer.Shutdown();
            return this;
        }
    }
}
