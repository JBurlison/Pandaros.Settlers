using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Extender
{
    public enum CraftPriority
    {
        Low = 0,
        Medium = 50,
        High = 100
    }

    public interface ICSRecipe : INameable
    {
        Dictionary<string, int> Requirements { get; }
        Dictionary<string, int> Results { get; }
        CraftPriority Priority { get; }
        bool IsOptional { get; }
        int DefautLimit { get; }
        string Job { get; }
    }
}
