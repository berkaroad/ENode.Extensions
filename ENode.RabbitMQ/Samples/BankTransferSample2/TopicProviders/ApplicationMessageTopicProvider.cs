using ENode.Messaging;
using ENode.EQueue;

namespace BankTransferSample2.Providers
{
    public class ApplicationMessageTopicProvider : AbstractTopicProvider<IApplicationMessage>
    {
        public override string GetTopic(IApplicationMessage applicationMessage)
        {
            return Constants.ApplicationMessageTopic;
        }
    }
}
