using System;
using System.Collections.Generic;
using System.Linq;

namespace Destr.Codegen.Source
{
    public class ClassSourceGenerator : SourceGenerator
    {
        public string Name;
        public string Namespace;
        public readonly SourceGenerator Attributes = new SourceGenerator();
        public readonly SourceGenerator Extends = new SourceGenerator();
        public readonly SourceGenerator Fields = new SourceGenerator();
        public readonly SourceGenerator Methods = new SourceGenerator();

        public ClassSourceGenerator(Type type) : this()
        {
            Name = SimpleName(type);
            Namespace = type.Namespace;
        }

        public ClassSourceGenerator()
        {
            Require(Attributes);
            Require(Extends);
            Require(Fields);
            Require(Methods);

            Add(Usings);
            Add(GenerateClassWithNamespace);
        }

        public override void Clear()
        {
            Attributes.Clear();
            Extends.Clear();
            Fields.Clear();
            Methods.Clear();
        }

        public MethodGenerator AddMethod(string name)
        {
            var method = new MethodGenerator(name);
            Methods.Add(method);
            return method;
        }

        private IEnumerable<string> Usings()
        {
            HashSet<string> namespaces = new HashSet<string>();
            foreach (var usingType in Dependence())
                if (!string.IsNullOrWhiteSpace(usingType.Namespace))
                    namespaces.Add(usingType.Namespace);
            foreach (var usingNamespaces in namespaces)
                if (usingNamespaces != Namespace)
                    yield return $"using {usingNamespaces};";
            yield break;
        }

        private IEnumerable<string> GenerateClassWithNamespace()
        {
            if (string.IsNullOrWhiteSpace(Namespace))
            {
                foreach (var line in GenerateClass(""))
                    yield return line;
            }
            else
            {
                yield return $"namespace {Namespace}";
                yield return "{";
                foreach (var line in GenerateClass($"{Space}"))
                    yield return line;
                yield return "}";
            }
        }

        private IEnumerable<string> GenerateClass(string offset)
        {
            var attributes = Attributes.GetSourceLines().ToArray();
            if (attributes.Length != 0)
                yield return $"{offset}[{string.Join(", ", attributes)}]";
            yield return string.Join("", GenerateClassDifinition(offset));
            yield return $"{offset}{{";
            foreach (var line in GenerateClassBody($"{offset}{Space}"))
                yield return line;
            yield return $"{offset}}}";
        }

        private IEnumerable<string> GenerateClassDifinition(string offset)
        {
            yield return offset;
            yield return "public class ";
            yield return Name;
            var extends = Extends.GetSourceLines().ToArray();
            if(extends.Length == 0)
                yield break;
            yield return " : ";
            yield return string.Join(",", extends);
        }

        protected IEnumerable<string> GenerateClassBody(string offset)
        {
            foreach (var line in Fields.GetSourceLines())
                yield return $"{offset}{line}";
            foreach (var line in Methods.GetSourceLines())
                yield return $"{offset}{line}";
        }

    }
}
