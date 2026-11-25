using System;
using System.Linq;

namespace BreachTactics
{
    public static class BreachAlgorithmFactory
    {
        public static T Create<T>(T fallback) where T : class
        {
            Type contract = typeof(T);
            Type implementation = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return Array.Empty<Type>();
                    }
                })
                .FirstOrDefault(type =>
                    contract.IsAssignableFrom(type)
                    && !type.IsAbstract
                    && !type.IsInterface
                    && type.Namespace == "BreachTactics.Student");

            if (implementation == null)
            {
                return fallback;
            }

            try
            {
                return Activator.CreateInstance(implementation) as T ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
