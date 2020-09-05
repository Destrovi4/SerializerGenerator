using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Destr.Codegen
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CodegenMethod : Attribute
    {
    }

    public abstract class CodeGenerator
    {
        private static readonly HashSet<Assembly> AssemblyList = new HashSet<Assembly>();
        private static HashSet<Type> _usedTypes = new HashSet<Type>();

        static CodeGenerator()
        {
            AssemblyList.Add(Assembly.GetExecutingAssembly());
        }

        public static void AddAssembly(Assembly assembly)
        {
            AssemblyList.Add(assembly);
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            return AssemblyList;
        }

        public static IEnumerable<Type> GetTypes()
        {
            return GetAssemblies().SelectMany(a => a.GetTypes());
        }

        public static void Generate()
        {
            List<Delegate> generationMethods = new List<Delegate>();

            foreach(var type in GetTypes())
            {
                foreach(var method in type.GetRuntimeMethods())
                {
                    var methodBody = method.GetMethodBody();
                    if (methodBody != null)
                    {
                        foreach (var localType in methodBody.LocalVariables
                            .Select(v=>v.LocalType)
                            .Where(t=>!t.ContainsGenericParameters))
                        {
                            _usedTypes.Add(localType);
                        }
                    }

                    var methodAttributes = method.GetCustomAttributes();
                    foreach(var methodAttribute in methodAttributes)
                    {
                        if(methodAttribute is CodegenMethod)
                        {
                            generationMethods.Add(method.CreateDelegate(typeof(Action)));
                        }
                    }
                }
            }

            foreach(var generationMethod in generationMethods)
            {
                generationMethod.DynamicInvoke();
            }
        }

        public static IEnumerable<Type> GetUsedTypes()
        {
            return _usedTypes;
        }
    }
}