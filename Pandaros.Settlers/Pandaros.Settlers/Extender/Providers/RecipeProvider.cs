using Pandaros.Settlers.Items;
using Recipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class RecipeProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSRecipe);
        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Recipes Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSRecipe recipe &&
                    !string.IsNullOrEmpty(recipe.Name))
                {
                    var requirements = new List<InventoryItem>();
                    var results = new List<ItemTypes.ItemTypeDrops>();

                    foreach (var ri in recipe.Requirements)
                        if (ItemTypes.IndexLookup.TryGetIndex(ri.Key, out var itemIndex))
                            requirements.Add(new InventoryItem(itemIndex, ri.Value));

                    foreach (var ri in recipe.Results)
                        if (ItemTypes.IndexLookup.TryGetIndex(ri.Key, out var itemIndex))
                            results.Add(new ItemTypes.ItemTypeDrops(itemIndex, ri.Value));

                    var newRecipe = new Recipe(recipe.Name, requirements, results, recipe.DefautLimit, recipe.IsOptional, (int)recipe.Priority);

                    if (recipe.IsOptional)
                        ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(recipe.Job, newRecipe);
                    else
                        ServerManager.RecipeStorage.AddDefaultLimitTypeRecipe(recipe.Job, newRecipe);

                    sb.Append($"{recipe.Name}, ");
                    i++;

                    if (i > 5)
                    {
                        sb.Append("</color>");
                        i = 0;
                        sb.AppendLine();
                        sb.Append("<color=lime>");
                    }
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }
    }
}
