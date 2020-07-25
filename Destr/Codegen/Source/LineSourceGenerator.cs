using System;
using System.Collections.Generic;
using System.Text;

namespace Destr.Codegen.Source
{
    public class LineSourceGenerator : SourceGenerator
    {
        public override IEnumerable<string> GetSourceLines()
        {
            yield return string.Join("", base.GetSourceLines());
        }
    }
}
