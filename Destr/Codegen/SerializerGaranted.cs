﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SerializerGenerator.Destr.Codegen
{
    [AttributeUsage(AttributeTargets.Struct)]
    class SerializerGaranted : Attribute
    {
        public readonly string File;
        public readonly string Member;
        public readonly int Line;
        public SerializerGaranted
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
