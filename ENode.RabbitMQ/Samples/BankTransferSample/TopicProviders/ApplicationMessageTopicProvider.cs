using ENode.Messaging;
using ENode.RabbitMQ;

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
