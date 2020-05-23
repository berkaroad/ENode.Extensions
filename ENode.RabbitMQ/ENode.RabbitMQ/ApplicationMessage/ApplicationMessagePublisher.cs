using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Serializing;
using ENode.Infrastructure;
using ENode.Messaging;
using RabbitMQTopic;
using TopicMessage = RabbitMQTopic.Message;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// Application Message Publisher
    /// </summary>
    public class ApplicationMessagePublisher : IMessagePublisher<IApplicationMessage>
    {
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<IApplicationMessage> _messageTopicProvider;
        private ITypeNameProvider _typeNameProvider;
        private SendQueueMessageService _sendMessageService;
        private Producer _producer;

        /// <summary>
        /// Initialize ENode
        /// </summary>
        /// <returns></returns>
        public ApplicationMessagePublisher InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageTopicProvider = ObjectContainer.Resolve<ITopicProvider<IApplicationMessage>>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _sendMessageService = new SendQueueMessageService();
            return this;
        }

        /// <summary>
        /// Initialize RabbitMQ
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="topic"></param>
        /// <param name="queueCount"></param>
        /// <param name="autoConfig"></param>
        /// <returns></returns>
        public ApplicationMessagePublisher InitializeRabbitMQ(ProducerSettings settings, string topic, int queueCount = 4, bool autoConfig = true)
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
        public ApplicationMessagePublisher Start()
        {
            _producer.Start();
            return this;
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        /// <returns></returns>
        public ApplicationMessagePublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }

        /// <summary>
        /// Publish message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task PublishAsync(IApplicationMessage message)
        {
            var queueMessage = CreateTopicMessage(message);
            return _sendMessageService.SendMessageAsync(_producer, "applicationMessage", message.GetType().Name, queueMessage, message.Id, message.Id, message.Items);
        }

        private TopicMessage CreateTopicMessage(IApplicationMessage message)
        {
            var topic = _messageTopicProvider.GetTopic(message);
            var data = _jsonSerializer.Serialize(message);
            return new TopicMessage(
                topic,
                (int)MessageTypeCode.ApplicationMessage,
                Encoding.UTF8.GetBytes(data),
                "text/json",
                _typeNameProvider.GetTypeName(message.GetType()));
        }
    }
}
