using ENode.Commanding;
using ENode.RabbitMQ;

namespace BankTransferSample.Providers
{
    public class CommandTopicProvider : AbstractTopicProvider<ICommand>
    {
        public override string GetTopic(ICommand command)
        {
            return Constants.CommandTopic;
        }
    }
}
