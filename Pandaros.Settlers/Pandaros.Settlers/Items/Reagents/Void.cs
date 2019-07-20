using Jobs;
using Pandaros.API.Entities;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class Void
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Items.Void.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Void";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "void.png");
            node["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("ingredient"));
            categories.AddToArray(new JSONNode("magic"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCCraftedRecipe, GameLoader.NAMESPACE + ".Items.Void.OnNPCCraftedRecipe")]
        public static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<RecipeResult> results)
        {
            if (recipe.Name == Elementium.Item.name && job.NPC != null)
            {
                var inv    = ColonistInventory.Get(job.NPC);
                var chance = 0.05f + inv.GetSkillModifier();

                if (Random.NextFloat() <= chance)
                {
                    inv.AddBonusProc(Item.ItemIndex);
                    results.Add(new RecipeResult(Item.ItemIndex));
                }
            }
        }
    }
}