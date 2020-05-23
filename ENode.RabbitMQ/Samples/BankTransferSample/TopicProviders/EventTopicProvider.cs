using ENode.Eventing;
using ENode.RabbitMQ;

namespace BankTransferSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return Constants.EventTopic;
        }
    }
}
