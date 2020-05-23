using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Serializing;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using RabbitMQTopic;
using TopicMessage = RabbitMQTopic.Message;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// DomainException Publisher
    /// </summary>
    public class DomainExceptionPublisher : IMessagePublisher<IDomainException>
    {
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<IDomainException> _exceptionTopicProvider;
        private ITypeNameProvider _typeNameProvider;
        private SendQueueMessageService _sendMessageService;
        private Producer _producer;

        /// <summary>
        /// Initialize ENode
        /// </summary>
        /// <returns></returns>
        public DomainExceptionPublisher InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _exceptionTopicProvider = ObjectContainer.Resolve<ITopicProvider<IDomainException>>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _sendMessageService = new SendQueueMessageService();
            return this;
        }

        /// <summary>
        /// InitializeRabbitMQ
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="topic"></param>
        /// <param name="queueCount"></param>
        /// <param name="autoConfig"></param>
        /// <returns></returns>
        public DomainExceptionPublisher InitializeRabbitMQ(ProducerSettings settings, string topic, int queueCount = 4, bool autoConfig = true)
        {
            InitializeENode();
            _producer = new Producer(settings, false, autoConfig);
            _producer.RegisterTopic(topic, queueCount);
            return this;
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <returns></returns>
        public DomainExceptionPublisher Start()
        {
            _producer.Start();
            return this;
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        /// <returns></returns>
        public DomainExceptionPublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }

        /// <summary>
        /// Publish message.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public Task PublishAsync(IDomainException exception)
        {
            var message = CreateTopicMessage(exception);
            return _sendMessageService.SendMessageAsync(_producer, "exception", exception.GetType().Name, message, exception.Id, exception.Id, exception.Items);
        }

        private TopicMessage CreateTopicMessage(IDomainException exception)
        {
            var topic = _exceptionTopicProvider.GetTopic(exception);
            var serializableInfo = new Dictionary<string, string>();
            exception.SerializeTo(serializableInfo);
            var data = _jsonSerializer.Serialize(new DomainExceptionMessage
            {
                UniqueId = exception.Id,
                Timestamp = exception.Timestamp,
                Items = exception.Items,
                SerializableInfo = serializableInfo
            });
            return new TopicMessage(topic, (int)MessageTypeCode.ExceptionMessage, Encoding.UTF8.GetBytes(data), "text/json", _typeNameProvider.GetTypeName(exception.GetType()));
        }
    }
}
