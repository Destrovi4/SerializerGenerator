using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Destr.Codegen.Source
{
    public class SwitchSourceGenerator : BlockGenerator
    {
        public readonly LineSourceGenerator Header = new LineSourceGenerator();

        public SwitchSourceGenerator()
        {
            Require(Header.Dependence);
            isBordered = true;
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
            foreach (var line in base.GetSourceLines())
                yield return line;
        }
    }
}
