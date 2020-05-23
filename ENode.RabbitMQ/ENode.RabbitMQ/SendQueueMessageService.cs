using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using RabbitMQTopic;

namespace ENode.RabbitMQ
{
    internal class SendQueueMessageService
    {
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public SendQueueMessageService()
        {
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
        }

        public async Task SendMessageAsync(Producer producer, string messageCategory, string messageTypeName, Message message, string routingKey, string messageId, IDictionary<string, string> messageExtensionItems)
        {
            try
            {
                var sendResult = await producer.SendMessageAsync(message, routingKey);
                if (sendResult.SendStatus == SendStatus.Success)
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("ENode {0} message send success, message: {1}, routingKey: {2}, messageType: {3}, messageId: {4}, broker messageId: {5}, messageExtensionItems: {6}",
                            messageCategory,
                            message,
                            routingKey,
                            messageTypeName,
                            messageId,
                            sendResult.MessageStoreResult.MessageId,
                            _jsonSerializer.Serialize(messageExtensionItems)
                        );
                    }
                }
                else
                {
                    throw new IOException($"Send message fail: {sendResult.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("ENode {0} message send has exception, message: {1}, routingKey: {2}, messageType: {3}, messageId: {4}, messageExtensionItems: {5}",
                    messageCategory,
                    message,
                    routingKey,
                    messageTypeName,
                    messageId,
                    _jsonSerializer.Serialize(messageExtensionItems)
                ), ex);
                throw new IOException("Send message has exception.", ex);
            }
        }
    }
}
