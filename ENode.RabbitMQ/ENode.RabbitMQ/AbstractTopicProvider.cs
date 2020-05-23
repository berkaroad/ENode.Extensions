using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// AbstractTopicProvider 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractTopicProvider<T> : ITopicProvider<T>
    {
        private readonly IDictionary<Type, string> _topicDict = new Dictionary<Type, string>();

        /// <summary>
        /// Get Topic
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual string GetTopic(T source)
        {
            return _topicDict[source.GetType()];
        }

        /// <summary>
        /// GetAllTopics
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTopics()
        {
            return _topicDict.Values.Distinct();
        }

        /// <summary>
        /// Get all source types
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<Type> GetAllTypes()
        {
            return _topicDict.Keys;
        }

        /// <summary>
        /// Register Topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="types"></param>
        protected void RegisterTopic(string topic, params Type[] types)
        {
            foreach (var type in types)
            {
                _topicDict.Add(type, topic);
            }
        }
    }
}
