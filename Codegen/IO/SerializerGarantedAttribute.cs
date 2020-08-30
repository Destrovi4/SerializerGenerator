using System;
using System.Runtime.CompilerServices;

namespace Destr.Codegen
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class SerializerGarantedAttribute : Attribute
    {
        public readonly string File;
        public readonly string Member;
        public readonly int Line;
        public SerializerGarantedAttribute
        (
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            File = file;
            Member = member;
            Line = line;
        }

        public override string ToString() => File + "(" + Line + "):" + Member;
    }
}
