using System;
using System.Collections.Generic;
using System.Linq;


namespace Destr.Codegen.Source
{
    public class MethodGenerator : BlockGenerator
    {
        public string ReturnType;
        public string Name;
        public bool isStatic = false;
        public bool isPublic = false;
        public bool isPrivate = false;
        public bool isOverride = false;
        private readonly SourceGenerator Arguments = new SourceGenerator();
        public readonly SourceGenerator Where = new SourceGenerator();

        public MethodGenerator(string name)
        {
            isBordered = true;
            Name = name;
            Require(Arguments.Dependence);
            Require(Where.Dependence);
        }

        public MethodGenerator SetStatic(bool value)
        {
            isStatic = value;
            return this;
        }

        public MethodGenerator Static
        {
            get => SetStatic(true);
        }

        public MethodGenerator SetPublic(bool value)
        {
            isPublic = value;
            return this;
        }

        public MethodGenerator Public
        {
            get => SetPublic(true);
        }

        public MethodGenerator SetPrivate(bool value)
        {
            isPrivate = value;
            return this;
        }

        public MethodGenerator Private
        {
            get => SetPrivate(true);
        }

        public MethodGenerator SetOverride(bool value)
        {
            isOverride = value;
            return this;
        }

        public MethodGenerator Override
        {
            get => SetOverride(true);
        }

        public MethodGenerator Return(string returnType)
        {
            ReturnType = returnType;
            return this;
        }

        public MethodGenerator Return(Type returnType)
        {
            if (returnType == null)
            {
                ReturnType = "void";
            }
            else
            {
                Require(returnType);
                ReturnType = RealTypeName(returnType);
            }
            return this;
        }

        public MethodGenerator Void
        {
            get => Return(null as Type);
        }

        public MethodGenerator Return<T>()
        {
            Return(typeof(Type));
            return this;
        }

        public MethodGenerator AddArgument(string arg)
        {
            Arguments.Add(arg);
            return this;
        }

        public ArgumentGenerator MakeArgument()
        {
            ArgumentGenerator generator = new ArgumentGenerator();
            Arguments.Add(generator);
            return generator;
        }

        public ArgumentGenerator Argument
        {
            get => MakeArgument();
        }

        public MethodGenerator AddArgument(string name, Type type)
        {
            Arguments.AddLine().Add(type).Add($" {name}");
            return this;
        }

        public MethodGenerator AddArgument(string name, Type type, params Type[] args)
        {
            Arguments.AddLine().Add(type, args).Add($" {name}");
            return this;
        }

        public MethodGenerator AddArgument<T>(string name)
        {
            return AddArgument(name, typeof(T));
        }

        public MethodGenerator AddWhere(string line)
        {
            Where.AddLine(line);
            return this;
        }

        public override IEnumerable<string> GetSourceLines()
        {
            yield return string.Join("", GenerateMethodDefinition());
            foreach (var line in base.GetSourceLines()) yield return line;
        }

        private IEnumerable<string> GenerateMethodDefinition()
        {
            if (isStatic) yield return "static ";
            if (isPublic) yield return "public ";
            if (isPrivate) yield return "private ";
            if (isOverride) yield return "override ";
            if (!string.IsNullOrEmpty(ReturnType)) yield return $"{ReturnType} ";
            yield return Name;
            yield return $"({string.Join(", ", Arguments.GetSourceLines())})";
            var wheres = Where.GetSourceLines().ToArray();
            if (wheres.Length > 0)
                foreach (var line in wheres)
                    yield return $" where {line}";
        }
    }
}