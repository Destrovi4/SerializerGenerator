using System.Collections.Generic;

namespace Destr.Codegen.Source
{
    public class ArgumentGenerator : SourceGenerator
    {
        public bool isIn = false;
        public bool isOut = false;

        public ArgumentGenerator SetIn(bool value)
        {
            isIn = value;
            return this;
        }

        public ArgumentGenerator In => SetIn(true);

        public ArgumentGenerator SetOut(bool value)
        {
            isOut = value;
            return this;
        }

        public ArgumentGenerator Out => SetOut(true);

        public ArgumentGenerator Ref
        {
            get
            {
                isIn = true;
                isOut = true;
                return this;
            }
        }

        public override IEnumerable<string> GetSourceLines()
        {
            yield return string.Join(" ", Generate());
        }

        private IEnumerable<string> Generate()
        {
            if (isIn && isOut)
                yield return "ref";
            else if (isIn)
                yield return "in";
            else if (isOut)
                yield return "out";
            foreach (var line in base.GetSourceLines())
                yield return line;
        }
    }
}
