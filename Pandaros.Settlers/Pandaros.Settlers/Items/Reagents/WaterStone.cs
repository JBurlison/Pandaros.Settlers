using Pandaros.API;
using Pandaros.Settlers.Jobs;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class WaterStone
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Items.WaterStone.Register")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void Register()
        {
            var aether = new InventoryItem(Elementium.Item.ItemIndex, 1);
            var torch  = new InventoryItem(ColonyBuiltIn.ItemTypes.BUCKETWATER.Id, 2);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {aether, torch},
                                    new RecipeResult(Item.ItemIndex, 1),
                                    6);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
            ServerManager.RecipeStorage.AddScienceRequirement(recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes,
            GameLoader.NAMESPACE + ".Items.WaterStone.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".WaterStone";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "Waterstone.png");
            node["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("ingredient"));
            categories.AddToArray(new JSONNode("magic"));
            categories.AddToArray(new JSONNode("stone"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }
    }
}