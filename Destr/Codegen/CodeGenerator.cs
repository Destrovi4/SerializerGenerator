//#define PRINT_TO_FILE

using System;
using System.Collections.Generic;
using System.IO;
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


        /*
        public static void Generate() => new CodeGenerator();

        private CodeGenerator()
        {
#if ALL_ASSEMBLY
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    Generate(assembly);
#else
            Generate(Assembly.GetExecutingAssembly());
#endif
        }

        private void Generate(Assembly assembly)
        {
            List<(Type, Generated)> toGenerate = new List<(Type, Generated)>();
            List<ICodeGenerator> generators = new List<ICodeGenerator>();

            foreach (Type type in assembly.GetTypes())
            {
                var generated = type.GetCustomAttribute<Generated>();
                if (generated != null)
                    toGenerate.Add((type, generated));
                if (type.GetInterfaces().Any(i => i == typeof(ICodeGenerator)))
                    generators.Add((ICodeGenerator)Activator.CreateInstance(type));
            }

            foreach ((Type generatedType, Generated generated) in toGenerate)
            {
                foreach(var generator in generators)
                {
                    if (!generator.ValidType(generatedType))
                        continue;
#if PRINT_TO_FILE
                    using var file = File.Open(generated.File, FileMode.OpenOrCreate, FileAccess.Write);
                    using var writer = new StreamWriter(file);
                    foreach (var line in generator.Generate(generatedType))
                        writer.WriteLine(line);
#else
                    Console.WriteLine("File: " + generated.File);
                    foreach (var line in generator.Generate(generatedType))
                        Console.WriteLine(line);
#endif
                }
            }
        }
        */
    }
}
