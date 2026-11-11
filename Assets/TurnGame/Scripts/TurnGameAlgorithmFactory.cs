using System;
using System.Linq;

namespace TurnGame
{
    public static class TurnGameAlgorithmFactory
    {
        public static T Create<T>(T fallback) where T : class
        {
            Type interfaceType = typeof(T);
            Type implementation = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface
                    && t.Namespace == "TurnGame.Student");

            if (implementation == null) return fallback;
            try { return Activator.CreateInstance(implementation) as T ?? fallback; }
            catch { return fallback; }
        }
    }
}
