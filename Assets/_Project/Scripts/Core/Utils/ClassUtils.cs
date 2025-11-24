using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utils
{
    public static class ClassUtils
    {
        public static T CreateInstance<T>(params object[] args) where T : class
        {
            var constructor = typeof(T).GetConstructors()[0];
            
            var parameters = constructor.GetParameters();
            
            var finalArgs = new List<object>();

            foreach (var parameter in parameters)
            {
                var matchingArg = args.FirstOrDefault(arg => parameter.ParameterType.IsInstanceOfType(arg));
                
                finalArgs.Add(matchingArg);
            }

            return (T)Activator.CreateInstance(typeof(T), finalArgs.ToArray());
        }
    }
}