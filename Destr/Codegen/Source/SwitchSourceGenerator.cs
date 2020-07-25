using System;
using System.Collections.Generic;
using System.Text;

namespace Destr.Codegen.Source
{
    public class SwitchSourceGenerator : SourceGenerator
    {
        public readonly LineSourceGenerator Header = new LineSourceGenerator();

        public SwitchSourceGenerator()
        {
            Require(Header.Dependence);
        }

        public CaseSourceGenerator AddCase()
        {
            var caseGenerator = new CaseSourceGenerator();
            Add(caseGenerator);
            return caseGenerator;
        }

        public CaseSourceGenerator AddCase(string header)
        {
            var caseGenerator = AddCase();
            caseGenerator.Header.Add(header);
            return caseGenerator;
        }

        public CaseSourceGenerator Case => AddCase();

        public override IEnumerable<string> GetSourceLines()
        {
            yield return $"switch({string.Join("", Header.GetSourceLines())})";
            yield return "{";
            foreach (var line in base.GetSourceLines())
                yield return $"{Space}{line}";
            yield return "}";
        }
    }
}
