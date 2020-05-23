using System;
using System.Collections.Generic;
using ENode.Commanding;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// Delayed Command
    /// </summary>
    [Serializable]
    public sealed class DelayedCommand : ICommand
    {
        private ICommand _command;

        /// <summary>
        /// Delayed Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="delayedMilliseconds"></param>
        public DelayedCommand(ICommand command, int delayedMilliseconds)
        {
            _command = command;
            DelayedMilliseconds = delayedMilliseconds;
        }

        /// <summary>
        /// Delayed milliseconds to send command
        /// </summary>
        public int DelayedMilliseconds { get; private set; }

        /// <summary>
        /// Get wrapped command
        /// </summary>
        /// <returns></returns>
        public ICommand GetWrappedCommand()
        {
            return _command;
        }

        /// <summary>
        /// AggregateRoot Id
        /// </summary>
        public string AggregateRootId => _command.AggregateRootId;

        /// <summary>
        /// Represents the unique identifier of the message.
        /// </summary>
        public string Id
        {
            get { return _command.Id; }
            set { _command.Id = value; }
        }

        /// <summary>
        /// Represents the timestamp of the message.
        /// </summary>
        public DateTime Timestamp
        {
            get { return _command.Timestamp; }
            set { _command.Timestamp = value; }
        }

        /// <summary>
        /// Represents the extension key/values data of the message.
        /// </summary>
        public IDictionary<string, string> Items
        {
            get { return _command.Items; }
            set { _command.Items = value; }
        }

        /// <summary>
        /// Merge the givens key/values into the current Items.
        /// </summary>
        /// <param name="items"></param>
        public void MergeItems(IDictionary<string, string> items)
        {
            _command.MergeItems(items);
        }
    }
}