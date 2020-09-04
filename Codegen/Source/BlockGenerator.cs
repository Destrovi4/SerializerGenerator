using System.Collections.Generic;


namespace Destr.Codegen.Source
{
    public class BlockGenerator : SourceGenerator
    {
        public bool isBordered = false;

        public BlockGenerator SetBordered(bool value)
        {
            isBordered = value;
            return this;
        }

        public BlockGenerator Bordered
        {
            get => SetBordered(true);
        }

        public BlockGenerator AddBlock()
        {
            var block = new BlockGenerator();
            Add(block);
            return block;
        }

        public BlockGenerator Block
        {
            get => AddBlock();
        }

        public SwitchSourceGenerator AddSwitch()
        {
            var generator = new SwitchSourceGenerator();
            Add(generator);
            return generator;
        }

        public SwitchSourceGenerator AddSwitch(string header)
        {
            var generator = AddSwitch();
            generator.Header.Add(header);
            return generator;
        }

        public override IEnumerable<string> GetSourceLines()
        {
            if (isBordered)
            {
                yield return "{";
                foreach (var line in base.GetSourceLines()) yield return $"{Space}{line}";
                yield return "}";
            }
            else
            {
                foreach (var line in base.GetSourceLines()) yield return line;
            }
        }
    }
}