using System;
using System.Collections.Generic;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// EventStream Message
    /// </summary>
    [Serializable]
    public class EventStreamMessage
    {
        /// <summary>
        /// MessageId
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// AggregateRootId
        /// </summary>
        public string AggregateRootId { get; set; }

        /// <summary>
        /// AggregateRootTypeName
        /// </summary>
        public string AggregateRootTypeName { get; set; }
        
        /// <summary>
        /// Event version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Related CommandId
        /// </summary>
        public string CommandId { get; set; }

        /// <summary>
        /// Events
        /// </summary>
        public IDictionary<string, string> Events { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public IDictionary<string, string> Items { get; set; }
    }
}
