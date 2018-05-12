using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs;
using Pipliz;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManagerAttribute]
    public static class Esper
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterAddingBaseTypes,
            GameLoader.NAMESPACE + ".Items.Esper.Add")]
        [ModLoader.ModCallbackDependsOnAttribute("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Esper";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "purplebottle.png");
            node["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("ingredient"));
            categories.AddToArray(new JSONNode("magic"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnNPCCraftedRecipe,
            GameLoader.NAMESPACE + ".Items.Esper.OnNPCCraftedRecipe")]
        public static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<InventoryItem> results)
        {
            if (recipe.Name == Mana.Item.name && job.NPC != null)
            {
                var inv    = SettlerInventory.GetSettlerInventory(job.NPC);
                var chance = 0.03f;

                if (inv.JobSkills.ContainsKey(ApothecaryRegister.JOB_NAME))
                    chance += inv.JobSkills[ApothecaryRegister.JOB_NAME];

                if (Random.NextFloat() <= chance)
                    results.Add(new InventoryItem(Item.ItemIndex));
            }
        }
    }
}