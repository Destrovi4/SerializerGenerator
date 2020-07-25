using System;
using System.Collections.Generic;

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

        public MethodGenerator(string name)
        {
            isBordered = true;
            Name = name;
            Require(Arguments.Dependence);
        }

        public MethodGenerator SetStatic(bool value)
        {
            isStatic = value;
            return this;
        }

        public MethodGenerator Static => SetStatic(true);

        public MethodGenerator SetPublic(bool value)
        {
            isPublic = value;
            return this;
        }

        public MethodGenerator Public => SetPublic(true);

        public MethodGenerator SetPrivate(bool value)
        {
            isPrivate = value;
            return this;
        }

        public MethodGenerator Private => SetPrivate(true);

        public MethodGenerator SetOverride(bool value)
        {
            isOverride = value;
            return this;
        }

        public MethodGenerator Override => SetOverride(true);

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

        public MethodGenerator Void => Return(null as Type);

        public MethodGenerator Return<T>()
        {
            Return(typeof(Type));
            return this;
        }

        public MethodGenerator Argument(string arg)
        {
            Arguments.Add(arg);
            return this;
        }

        public MethodGenerator Argument(string name, Type type)
        {
            Arguments.AddLine().Add(type).Add($" {name}");
            return this;
        }

        public MethodGenerator Argument(string name, Type type, params Type[] args)
        {
            Arguments.AddLine().Add(type, args).Add($" {name}");
            return this;
        }

        public MethodGenerator Argument<T>(string name) => Argument(name, typeof(T));

        public override IEnumerable<string> GetSourceLines()
        {
            yield return string.Join("", GenerateMethodDifinition());
            foreach (var line in base.GetSourceLines())
                yield return line;
        }

        private IEnumerable<string> GenerateMethodDifinition()
        {
            if (isStatic)
                yield return "static ";
            if (isPublic)
                yield return "public ";
            if (isPrivate)
                yield return "private ";
            if (isOverride)
                yield return "override ";
            if (!string.IsNullOrEmpty(ReturnType))
                yield return $"{ReturnType} ";
            yield return Name;
            yield return $"({string.Join(", ", Arguments.GetSourceLines())})";
        }
    }
}
