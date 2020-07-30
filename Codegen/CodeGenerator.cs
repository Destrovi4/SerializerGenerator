using System;
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
        public static void Generate()
        {
            var generators = Assembly
               .GetAssembly(typeof(ICodeGenerator))
               .GetTypes()
               .Where(t => !t.IsAbstract)
               .Where(t => t.GetInterfaces().Any(i => i == typeof(ICodeGenerator)))
               .Select(t => Activator.CreateInstance(t) as ICodeGenerator);
            foreach(var generate in generators)
                generate.Generate();
        }
    }
}
