using System;

namespace Assets.SerializerGenerator.Codegen
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class GenericArdumentsAttribute : Attribute
    {
        public readonly Type[] Types;

        public GenericArdumentsAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}
