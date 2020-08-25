using System;
using System.Runtime.CompilerServices;

namespace Destr.Codegen
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class Generated : Attribute
    {
        public readonly string File;
        public readonly string Member;
        public readonly int Line;
        public readonly Type Argument;
        public Generated
        (
            Type argument = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            File = file;
            Member = member;
            Line = line;
            Argument = argument;
        }

        public override string ToString() { return File + "(" + Line + "):" + Member; }
    }
}
