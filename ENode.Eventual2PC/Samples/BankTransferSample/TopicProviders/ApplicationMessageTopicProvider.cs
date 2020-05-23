using ENode.RabbitMQ;
using ENode.Infrastructure;
using ENode.Messaging;

namespace BankTransferSample.Providers
{
    public class ApplicationMessageTopicProvider : AbstractTopicProvider<IApplicationMessage>
    {
        public override string GetTopic(IApplicationMessage applicationMessage)
        {
            return Constants.ApplicationMessageTopic;
        }
    }
}
