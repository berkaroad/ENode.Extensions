﻿namespace ENode.RabbitMQ
{
    internal enum MessageTypeCode
    {
        CommandMessage = 1,
        DomainEventStreamMessage = 2,
        ExceptionMessage = 3,
        ApplicationMessage = 4,
    }
}
