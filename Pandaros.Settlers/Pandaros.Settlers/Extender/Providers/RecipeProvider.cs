using Pandaros.Settlers.Items;
using Recipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class RecipeProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSRecipe);
        public Type ClassType => null;

        public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            
        }

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Recipes Loaded----------------------");

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSRecipe recipe &&
                    !string.IsNullOrEmpty(recipe.Name))
                {
                    var requirements = new List<InventoryItem>();
                    var results = new List<InventoryItem>();

                    foreach (var ri in recipe.Requirements)
                        if (ItemTypes.IndexLookup.TryGetIndex(ri.Key, out var itemIndex))
                            requirements.Add(new InventoryItem(itemIndex, ri.Value));

                    foreach (var ri in recipe.Results)
                        if (ItemTypes.IndexLookup.TryGetIndex(ri.Key, out var itemIndex))
                            results.Add(new InventoryItem(itemIndex, ri.Value));

                    var newRecipe = new Recipe(recipe.Name, requirements, results, recipe.DefautLimit, recipe.IsOptional, recipe.DefautLimit);

                    if (recipe.IsOptional)
                        ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(recipe.Job, newRecipe);
                    else
                        ServerManager.RecipeStorage.AddDefaultLimitTypeRecipe(recipe.Job, newRecipe);

                    sb.Append($"{recipe.Name}, ");
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }

        public void AfterSelectedWorld()
        {
            
        }

        public void AfterWorldLoad()
        {
            
        }
        public void OnAddResearchables()
        {

        }
    }
}
