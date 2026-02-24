using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Core.Extensions
{
    public static class MethodConfigExtensions
    {
        public static void ConfigureMethod<T>(this T target, string methodName, params object[] parameters) where T : class
        {
            var methods = target.GetType().GetMethods()
                .Where(m => m.Name == methodName && m.GetParameters().Length == parameters.Length)
                .ToArray();

            MethodInfo configureMethod = null;
            
            foreach (var method in methods)
            {
                var methodParams = method.GetParameters();
                if (ValidateParameters(methodParams, parameters, out var error))
                {
                    configureMethod = method;
                    break;
                }
                
                if(!string.IsNullOrEmpty(error)) 
                    Debug.LogError(error);
            }

            if (configureMethod != null)
            {
                configureMethod.Invoke(target, parameters);
            }
            else
            {
                Debug.LogError($"Method {methodName} with matching parameters not found on {target.GetType().Name}.");
            }
        }

        private static bool ValidateParameters(ParameterInfo[] methodParams, object[] parameters, out string error)
        {
            if (methodParams.Length != parameters.Length)
            {
                error = $"Expected {methodParams.Length} parameters, but got {parameters.Length}.";
                return false;
            }

            for (int i = 0; i < methodParams.Length; i++)
            {
                if (parameters[i] == null || methodParams[i].ParameterType.IsInstanceOfType(parameters[i])) continue;

                error = $"Parameter {i + 1} expected type {methodParams[i].ParameterType.Name}, but got {parameters[i].GetType().Name}.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}