﻿using System;
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
        public readonly bool IsPartial;

        public Generated
        (
            Type argument = null,
            bool isPartial = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            File = file;
            Member = member;
            Line = line;
            Argument = argument;
            IsPartial = isPartial;
        }

        public override string ToString()
        {
            return $"{File}({Line}):{Member}";
        }
    }
}