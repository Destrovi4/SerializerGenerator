using Destr.Codegen.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Destr.Codegen
{
    public abstract class ClassGenerator : WriterCodeGenerator
    {
        protected override IEnumerable<(string, IEnumerable<string>)> GetSources()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Select(Select)
                .Where(Filter)
                .Where(FilterType)
                .Select(Generate);
        }

        private (Generated, Type) Select(Type type) => (type.GetCustomAttribute<Generated>(), type);
        private bool Filter((Generated, Type) tuple) => tuple.Item1 != null;
        private bool FilterType((Generated, Type) tuple) => FilterType(tuple.Item2);
        protected abstract bool FilterType(Type type);
        protected abstract IEnumerable<string> Generate(Type type);
        private (string, IEnumerable<string>) Generate((Generated, Type) tuple)
        {
            (Generated generated, Type type) = tuple;
            return (generated.File, Generate(type));
        }
    }
}
