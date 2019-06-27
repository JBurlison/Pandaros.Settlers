using Pandaros.Settlers.Items;
using Recipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class RecipeProvider : IAfterItemTypesDefined
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSRecipe);
        public Type ClassType => null;

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.LogToFile("-------------------Recipes Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSRecipe recipe &&
                    !string.IsNullOrEmpty(recipe.name))
                {
                    var requirements = new List<InventoryItem>();
                    var results = new List<RecipeResult>();
                    recipe.JsonSerialize();

                    foreach (var ri in recipe.requires)
                        if (ItemTypes.IndexLookup.TryGetIndex(ri.type, out var itemIndex))
                            requirements.Add(new InventoryItem(itemIndex, ri.amount));

                    foreach (var ri in recipe.results)
                            results.Add(ri);

                    var newRecipe = new Recipe(recipe.name, requirements, results, recipe.defaultLimit, 0, (int)recipe.defaultPriority);

                    ServerManager.RecipeStorage.AddLimitTypeRecipe(recipe.Job, newRecipe);

                    if (recipe.isOptional)
                        ServerManager.RecipeStorage.AddScienceRequirement(newRecipe);
                        
                    sb.Append($"{recipe.name}, ");
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            PandaLogger.LogToFile(sb.ToString());
            PandaLogger.LogToFile("---------------------------------------------------------");
        }
    }
}
