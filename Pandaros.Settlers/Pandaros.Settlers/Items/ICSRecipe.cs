using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    public enum CraftPriority
    {
        Low = -100,
        Medium = 0,
        High = 100
    }

    public class RecipeItem
    {
        public RecipeItem() { }
        public RecipeItem(string itemName, int count = 1)
        {
            type = itemName;
            amount = count;
        }

        public RecipeItem(ushort itemId, int count = 1)
        {
            type = ItemTypes.IndexLookup.IndexLookupTable[itemId];
            amount = count;
        }

        public string type { get; set; }
        public int amount { get; set; }
    }

    public interface ICSRecipe : INameable
    {
        List<RecipeItem> requires { get; }
        List<RecipeItem> results { get; }
        CraftPriority defaultPriority { get; }
        bool isOptional { get; }
        int defaultLimit { get; }
        string Job { get; }
    }
}
