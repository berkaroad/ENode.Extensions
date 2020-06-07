using ENode.Eventing;
using ENode.EQueue;

namespace BankTransferSample2.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return Constants.EventTopic;
        }
    }
}
