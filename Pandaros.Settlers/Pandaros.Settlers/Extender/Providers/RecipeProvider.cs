using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class RecipeProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSRecipe);

        public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            
        }

        public void AfterItemTypesDefined()
        {
            
        }

        public void AfterSelectedWorld()
        {
            
        }

        public void AfterWorldLoad()
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
                        if (ItemTypesServer.TryGetType(ri.Key, out var itemAction))
                            requirements.Add(new InventoryItem(itemAction.typeMain, ri.Value));

                    foreach (var ri in recipe.Results)
                        if (ItemTypesServer.TryGetType(ri.Key, out var itemAction))
                            requirements.Add(new InventoryItem(itemAction.typeMain, ri.Value));

                    var newRecipe = new Recipe(recipe.Name, requirements, results, recipe.DefautLimit, recipe.IsOptional, recipe.DefautLimit);

                    if (recipe.IsOptional)
                        RecipeStorage.AddOptionalLimitTypeRecipe(recipe.Job, newRecipe);
                    else
                        RecipeStorage.AddDefaultLimitTypeRecipe(recipe.Job, newRecipe);

                    sb.Append($"{recipe.Name}, ");
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }
    }
}
