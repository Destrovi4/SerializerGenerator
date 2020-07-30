using System;
using System.Runtime.CompilerServices;

namespace Destr.Codegen
{
    public class Generated : Attribute
    {
        public readonly string File;
        public readonly string Member;
        public readonly int Line;
        public Generated
        (
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            File = file;
            Member = member;
            Line = line;
        }

        public override string ToString() { return File + "(" + Line + "):" + Member; }
    }
}
