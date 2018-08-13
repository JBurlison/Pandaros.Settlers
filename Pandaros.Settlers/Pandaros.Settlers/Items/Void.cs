using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs;
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Void.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
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
        public static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<InventoryItem> results)
        {
            if (recipe.Name == Elementium.Item.name && job.NPC != null)
            {
                var inv    = SettlerInventory.GetSettlerInventory(job.NPC);
                var chance = 0.05f;

                if (inv.JobSkills.ContainsKey(ApothecaryRegister.JOB_NAME))
                    chance += inv.JobSkills[ApothecaryRegister.JOB_NAME];

                if (Random.NextFloat() <= chance)
                {
                    results.Add(new InventoryItem(Item.ItemIndex));

                    PandaChat.Send(job.NPC.Colony.Owner,
                                   $"{inv.SettlerName} the Apothecary has discovered a Void Stone while crafting Elementium!",
                                   ChatColor.orange);
                }
            }
        }
    }
}