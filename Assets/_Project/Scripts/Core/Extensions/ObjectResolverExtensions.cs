using System;
using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace Core.Extensions
{
    public static class ObjectResolverExtensions
    {
        public static T CreateInstance<T>(this IObjectResolver container) where T : class
        {
            var constructor = typeof(T).GetConstructors()[0];
            
            var parameters = constructor.GetParameters();
            
            var finalArgs = new List<object>();

            foreach (var parameter in parameters)
            {
                finalArgs.Add(container.Resolve(parameter.ParameterType));
            }

            return (T)Activator.CreateInstance(typeof(T), finalArgs.ToArray());
        }
        
        public static T CreateInstance<T>(this IObjectResolver container, params object[] args) where T : class
        {
            var constructor = typeof(T).GetConstructors()[0];
            
            var parameters = constructor.GetParameters();
            
            var finalArgs = new List<object>();

            foreach (var parameter in parameters)
            {
                var matchingArg = args.FirstOrDefault(arg => parameter.ParameterType.IsInstanceOfType(arg));
                
                finalArgs.Add(matchingArg ?? container.Resolve(parameter.ParameterType));
            }

            return (T)Activator.CreateInstance(typeof(T), finalArgs.ToArray());
        }
    }
}