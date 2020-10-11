using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class AssemblyResolver
{
    static AssemblyResolver()
    {
    }

    public static IEnumerable<Assembly> GetAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies();
    }

    public static IEnumerable<Type> GetTypes()
    {
        return GetAssemblies().SelectMany(a => a.GetTypes());
    }

    public static IEnumerable<Type> GetNotAbstractGenericClasses()
    {
        return GetTypes().Where(t => t.IsGenericType && t.IsAbstract && !t.IsInterface);
    }

    public static IEnumerable<Type> GetGenericUsages()
    {
        foreach(Type type in GetNotAbstractGenericClasses())
        {
            foreach(var method in type.GetRuntimeMethods())
            {
                var methodBody = method.GetMethodBody();
                
                if(methodBody == null)
                    continue;
                foreach (var localType in methodBody.LocalVariables.Select(v=>v.LocalType).Where(t=>t.ContainsGenericParameters))
                {
                    yield return localType;
                }
            }
        }
    }
}