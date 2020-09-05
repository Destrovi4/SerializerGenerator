using System;
using System.Linq;


namespace Destr.Codegen
{
    public static class TypeUtil
    {
        public static Type FindGenericInterface(this Type type, Type interfaceType)
        {
            if (!interfaceType.IsInterface) throw new Exception();
            return type.GetInterfaces().Where(i => i.IsGenericType).FirstOrDefault(i => i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}