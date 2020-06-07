using System.Threading;
using System.Threading.Tasks;
using BankTransferSample2.Domain;
using ENode.Messaging;

namespace BankTransferSample2.EventHandlers
{
    public class SyncHelper :
        IMessageHandler<DepositTransactionCompletedEvent>,
        IMessageHandler<TransferTransactionCompletedEvent>,
        IMessageHandler<TransferTransactionCanceledEvent>
    {
        private ManualResetEvent _waitHandle = new ManualResetEvent(false);

        public void WaitOne()
        {
            _waitHandle.WaitOne();
        }

        public Task HandleAsync(DepositTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCanceledEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.CompletedTask;
        }
    }
}
