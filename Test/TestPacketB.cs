using SerializerGenerator.Destr.Codegen;
using System;
using System.Collections.Generic;
using System.Text;

namespace SerializerGenerator.Test
{
    [SerializerGaranted]
    public struct TestPacketB
    {
        public TestPacketA a;
        public int b;
    }
}
