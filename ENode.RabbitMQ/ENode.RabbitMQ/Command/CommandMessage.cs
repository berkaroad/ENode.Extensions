using System;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// Command Message
    /// </summary>
    [Serializable]
    public class CommandMessage
    {
        /// <summary>
        /// Command Data
        /// </summary>
        public string CommandData { get; set; }

        /// <summary>
        /// ReplyAddress
        /// </summary>
        public string ReplyAddress { get; set; }

        /// <summary>
        /// Saga Id
        /// </summary>
        public string SagaId { get; set; }
    }
}
