using System;
using System.Collections.Generic;
using System.Text;

namespace Destr.Codegen.Source
{
    public class CaseSourceGenerator : SourceGenerator
    {
        public bool isDefault = false;
        public bool isBordered = false;
        public bool isBreak = false;
        public readonly LineSourceGenerator Header = new LineSourceGenerator();

        public CaseSourceGenerator()
        {
            Require(Header.Dependence);
        }

        public CaseSourceGenerator SetDefault(bool value)
        {
            isDefault = value;
            return this;
        }

        public CaseSourceGenerator Default => SetDefault(true);

        public CaseSourceGenerator SetBordered(bool value)
        {
            isBordered = value;
            return this;
        }

        public CaseSourceGenerator Bordered => SetBordered(true);

        public CaseSourceGenerator SetBreak(bool value)
        {
            isBreak = value;
            return this;
        }

        public CaseSourceGenerator Break => SetBreak(true);

        public override IEnumerable<string> GetSourceLines()
        {
            if (isDefault)
                yield return "default:";
            else 
                yield return $"case {string.Join("", Header.GetSourceLines())}:";
            if (isBordered)
                yield return "{";
            foreach (var line in base.GetSourceLines())
                    yield return $"{Space}{line}";
            if (isBordered)
                yield return "}";
            if(isBreak)
                yield return "break;";
        }
    }
}
