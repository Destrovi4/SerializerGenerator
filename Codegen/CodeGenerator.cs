using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Destr.Codegen
{

    public interface ICodeGenerator
    {
        void Generate();
    }


    public abstract class CodeGenerator
    {
        private static readonly HashSet<Assembly> AssemblyList = new HashSet<Assembly>();
        private static HashSet<Type> _usedTypes = null;

        static CodeGenerator()
        {
            AssemblyList.Add(Assembly.GetExecutingAssembly());
        }

        public static void Generate()
        {
            var generators = AssemblyList
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i => i == typeof(ICodeGenerator)))
                .Select(t => Activator.CreateInstance(t) as ICodeGenerator);
            foreach (var generate in generators) generate.Generate();
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

        public static IEnumerable<Type> GetUsedTypes()
        {
            if(_usedTypes == null)
            {
                _usedTypes = new HashSet<Type>();
                foreach(var type in GetTypes()
                    .SelectMany(t=>t.GetRuntimeMethods())
                    .Select(m=>m.GetMethodBody())
                    .Where(b=>b!=null && b.LocalVariables != null)
                    .SelectMany(b=>b.LocalVariables)
                    .Select(v=>v.LocalType)
                    .Where(t=>!t.IsGenericType || !t.GetGenericArguments().Any(a=>a == null))
                    )
                {
                    _usedTypes.Add(type);
                }
            }
            return _usedTypes;
        }
    }
}