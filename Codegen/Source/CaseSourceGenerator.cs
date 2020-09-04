using System.Collections.Generic;


namespace Destr.Codegen.Source
{
    public class CaseSourceGenerator : BlockGenerator
    {
        public bool isDefault = false;
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

        public CaseSourceGenerator Default
        {
            get => SetDefault(true);
        }

        public CaseSourceGenerator SetBreak(bool value)
        {
            isBreak = value;
            return this;
        }

        public CaseSourceGenerator Break
        {
            get => SetBreak(true);
        }

        public override IEnumerable<string> GetSourceLines()
        {
            if (isDefault)
                yield return "default:";
            else
                yield return $"case {string.Join("", Header.GetSourceLines())}:";
            if (isBordered)
                foreach (var line in base.GetSourceLines())
                    yield return line;
            else
                foreach (var line in base.GetSourceLines())
                    yield return $"{Space}{line}";
            if (isBreak) yield return "break;";
        }
    }
}