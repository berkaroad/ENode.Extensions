using System;
using System.Collections.Generic;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// DomainException Message
    /// </summary>
    [Serializable]
    public class DomainExceptionMessage
    {
        /// <summary>
        /// UniqueId
        /// </summary>
        public string UniqueId { get; set; }
        
        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Items
        /// </summary>
        public IDictionary<string, string> Items { get; set; }

        /// <summary>
        /// SerializableInfo
        /// </summary>
        public IDictionary<string, string> SerializableInfo { get; set; }
    }
}
