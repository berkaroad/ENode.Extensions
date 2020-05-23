﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ECommon.Components;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.RabbitMQ
{
    /// <summary>
    /// ENode Extensions
    /// </summary>
    public static class ENodeExtensions
    {
        /// <summary>
        /// Register Topic Provider
        /// </summary>
        /// <param name="enodeConfiguration"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static ENodeConfiguration RegisterTopicProviders(this ENodeConfiguration enodeConfiguration, params Assembly[] assemblies)
        {
            var registeredTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(x => IsTopicProviderType(x)))
                {
                    RegisterComponentType(type);
                    registeredTypes.Add(type);
                }
                foreach (var type in assembly.GetTypes().Where(TypeUtils.IsComponent))
                {
                    if (!registeredTypes.Contains(type))
                    {
                        RegisterComponentType(type);
                    }
                }
            }
            return enodeConfiguration;
        }

        /// <summary>
        /// Send a delayed command asynchronously.
        /// </summary>
        /// <param name="commandService"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Task SendDelayedCommandAsync(this ICommandService commandService, DelayedCommand command)
        {
            return commandService.SendAsync(command);
        }

        /// <summary>
        /// Execute a delayed command asynchronously with the default command return type.
        /// </summary>
        /// <param name="commandService"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Task ExecuteDelayedCommandAsync(this ICommandService commandService, DelayedCommand command)
        {
            return commandService.ExecuteAsync(command);
        }

        /// <summary>
        /// Execute a delayed command asynchronously with the specified command return type.
        /// </summary>
        /// <param name="commandService"></param>
        /// <param name="command"></param>
        /// <param name="commandReturnType"></param>
        /// <returns></returns>
        public static Task ExecuteDelayedCommandAsync(this ICommandService commandService, DelayedCommand command, CommandReturnType commandReturnType)
        {
            return commandService.ExecuteAsync(command, commandReturnType);
        }

        private static bool IsTopicProviderType(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITopicProvider<>));
        }
        private static void RegisterComponentType(Type type)
        {
            var life = ParseComponentLife(type);
            ObjectContainer.RegisterType(type, null, life);
            foreach (var interfaceType in type.GetInterfaces())
            {
                ObjectContainer.RegisterType(interfaceType, type, null, life);
            }
        }
        private static LifeStyle ParseComponentLife(Type type)
        {
            var attributes = type.GetCustomAttributes<ComponentAttribute>(false);
            if (attributes != null && attributes.Any())
            {
                return attributes.First().LifeStyle;
            }
            return LifeStyle.Singleton;
        }
    }
}