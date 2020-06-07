using ENode.Commanding;
using ENode.EQueue;

namespace BankTransferSample2.Providers
{
    public class CommandTopicProvider : AbstractTopicProvider<ICommand>
    {
        public override string GetTopic(ICommand command)
        {
            return Constants.CommandTopic;
        }
    }
}
