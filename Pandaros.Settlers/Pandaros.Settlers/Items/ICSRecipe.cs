using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    public enum CraftPriority
    {
        Low = 0,
        Medium = 50,
        High = 100
    }

    public interface ICSRecipe : INameable
    {
        Dictionary<ItemId, int> Requirements { get; }
        Dictionary<ItemId, int> Results { get; }
        CraftPriority Priority { get; }
        bool IsOptional { get; }
        int DefautLimit { get; }
        string Job { get; }
    }
}
