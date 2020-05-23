using System;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Infrastructure;
using ENode.Messaging;
using RabbitMQTopic;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// ApplicationMessage Consumer
    /// </summary>
    public class ApplicationMessageConsumer
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
        public ApplicationMessageConsumer InitializeENode()
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
        public ApplicationMessageConsumer InitializeRabbitMQ(ConsumerSettings settings, bool autoConfig = true)
        {
            InitializeENode();
            _consumer = new Consumer(settings, autoConfig);
            return this;
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <returns></returns>
        public ApplicationMessageConsumer Start()
        {
            _consumer.OnMessageReceived += (sender, e) =>
            {
                try
                {
                    var applicationMessageType = _typeNameProvider.GetType(e.Context.GetMessageType());
                    var message = _jsonSerializer.Deserialize(Encoding.UTF8.GetString(e.Context.GetBody()), applicationMessageType) as IApplicationMessage;
                    _logger.DebugFormat("ENode application message received, messageId: {0}, messageType: {1}", message.Id, message.GetType().Name);

                    _messageDispatcher.DispatchMessageAsync(message).ContinueWith(x =>
                    {
                        e.Context.Ack();
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"ENode application message handle failed: {ex.Message}, body: {Encoding.UTF8.GetString(e.Context.GetBody())}, messageType: {e.Context.GetMessageType()}", ex);
                }
            };
            _consumer.Start();
            return this;
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="queueCount"></param>
        /// <returns></returns>
        public ApplicationMessageConsumer Subscribe(string topic, int queueCount = 4)
        {
            _consumer.Subscribe(topic, queueCount);
            return this;
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        /// <returns></returns>
        public ApplicationMessageConsumer Shutdown()
        {
            _consumer.Shutdown();
            return this;
        }
    }
}
