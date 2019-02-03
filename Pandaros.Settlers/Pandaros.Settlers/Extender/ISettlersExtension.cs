using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Extender
{
    public interface ISettlersExtension
    {
        List<Type> LoadedAssembalies { get; }

        string InterfaceName { get; }

        Type ClassType { get; }
    }
}
