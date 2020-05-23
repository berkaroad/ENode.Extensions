using ENode.Domain;
using ENode.RabbitMQ;

namespace BankTransferSample.Providers
{
    public class ExceptionTopicProvider : AbstractTopicProvider<IDomainException>
    {
        public override string GetTopic(IDomainException source)
        {
            return Constants.ExceptionTopic;
        }
    }
}
