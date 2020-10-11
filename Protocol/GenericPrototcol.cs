using System;
using System.Collections.Generic;
using System.Linq;

namespace Destr.Protocol
{
    public class AbstractProtocol<T> : IProtocol<T> where T : IProtocol<T>
    {
        private static Type[] _packetTypes;
        private static string _staticDefinition;
        static AbstractProtocol()
        {
            HashSet<Type> packetsSet = new HashSet<Type>();
            foreach (Type type in AssemblyResolver.GetGenericUsages())
            {
                if (type.GetGenericTypeDefinition() is IPacket<T>)
                {
                    packetsSet.Add(type);
                }

                foreach (Type genericArgument in type.GetGenericArguments())
                {
                    if (!(genericArgument is IPacket<T>))
                        continue;
                    packetsSet.Add(genericArgument);
                }
            }

            List<Type> packetList = packetsSet.ToList();
            packetList.Sort((a, b) => string.Compare(a.Name, b.Name));
            _packetTypes = packetList.ToArray();
            _staticDefinition = string.Join(";", _packetTypes.Select(t=>t.FullName));
        }

        public string Definition
        {
            get => _staticDefinition;
        }

        public IEnumerable<Type> GetPacketTypes()
        {
            return _packetTypes;
        }

    }
}