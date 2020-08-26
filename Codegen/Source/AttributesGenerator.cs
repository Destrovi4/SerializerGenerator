namespace Destr.Codegen.Source
{
    public class AttributesGenerator : SourceGenerator
    {
        public void Add(Generated generated)
        {
            var line = Line;
            line.Add<Generated>();
            if (generated.Argument == null && !generated.IsPartial)
                return;
            SourceGenerator arguments = new SourceGenerator();
            Require(arguments);
            arguments.Line.Add("argument: typeof(").Add(generated.Argument).Add(")");
            if (generated.IsPartial)
                arguments.Add("isPartial: true");
            line.Add($"({string.Join(",", arguments.GetSourceLines())})");
        }
    }
}
