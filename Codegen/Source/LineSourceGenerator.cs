using System.Collections.Generic;


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