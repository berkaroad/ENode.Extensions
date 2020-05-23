using ECommon.Components;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Eventing;
using ENode.Messaging;
using RabbitMQTopic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopicMessage = RabbitMQTopic.Message;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// DomainEvent Publisher
    /// </summary>
    public class DomainEventPublisher : IMessagePublisher<DomainEventStreamMessage>
    {
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<IDomainEvent> _eventTopicProvider;
        private IEventSerializer _eventSerializer;
        private SendQueueMessageService _sendMessageService;
        private Producer _producer;

        /// <summary>
        /// Initialize ENode
        /// </summary>
        /// <returns></returns>
        public DomainEventPublisher InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<ITopicProvider<IDomainEvent>>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
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
        public DomainEventPublisher InitializeRabbitMQ(ProducerSettings settings, string topic, int queueCount = 4, bool autoConfig = true)
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
        public DomainEventPublisher Start()
        {
            _producer.Start();
            return this;
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        /// <returns></returns>
        public DomainEventPublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }

        /// <summary>
        /// Publish message.
        /// </summary>
        /// <param name="eventStream"></param>
        /// <returns></returns>
        public Task PublishAsync(DomainEventStreamMessage eventStream)
        {
            var message = CreateTopicMessage(eventStream);
            return _sendMessageService.SendMessageAsync(_producer, "events", string.Join(",", eventStream.Events.Select(x => x.GetType().Name)), message, eventStream.AggregateRootId, eventStream.Id, eventStream.Items);
        }

        private TopicMessage CreateTopicMessage(DomainEventStreamMessage eventStream)
        {
            Ensure.NotNull(eventStream.AggregateRootId, "aggregateRootId");
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _jsonSerializer.Serialize(eventMessage);
            return new TopicMessage(topic, (int)MessageTypeCode.DomainEventStreamMessage, Encoding.UTF8.GetBytes(data), "text/json");
        }

        private EventStreamMessage CreateEventMessage(DomainEventStreamMessage eventStream)
        {
            var message = new EventStreamMessage
            {
                Id = eventStream.Id,
                CommandId = eventStream.CommandId,
                AggregateRootTypeName = eventStream.AggregateRootTypeName,
                AggregateRootId = eventStream.AggregateRootId,
                Timestamp = eventStream.Timestamp,
                Version = eventStream.Version,
                Events = _eventSerializer.Serialize(eventStream.Events),
                Items = eventStream.Items
            };

            return message;
        }
    }
}
