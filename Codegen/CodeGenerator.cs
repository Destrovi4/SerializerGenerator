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
        static CodeGenerator()
        {
            AssemblyList.Add(Assembly.GetExecutingAssembly());
        }

        public static void Generate()
        {
            var generators = AssemblyList
                .SelectMany(a=> a.GetTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i => i == typeof(ICodeGenerator)))
                .Select(t => Activator.CreateInstance(t) as ICodeGenerator);
            foreach(var generate in generators)
                generate.Generate();
        }

        public static void AddAssembly(Assembly assembly) => AssemblyList.Add(assembly);

        public static IEnumerable<Assembly> GetAssemblys() => AssemblyList;
    }
}
