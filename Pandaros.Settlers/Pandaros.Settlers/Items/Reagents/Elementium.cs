using BlockTypes;
using Pandaros.Settlers.Jobs;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class Elementium
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Items.Elementium.Register")]
        public static void Register()
        {
            var aether = new InventoryItem(Aether.Item.ItemIndex, 1);
            var copper = new InventoryItem(BuiltinBlocks.Copper, 400);
            var iron   = new InventoryItem(BuiltinBlocks.IronOre, 200);
            var tin    = new InventoryItem(BuiltinBlocks.Tin, 400);
            var gold   = new InventoryItem(BuiltinBlocks.GoldOre, 100);
            var silver = new InventoryItem(BuiltinBlocks.GalenaSilver, 50);
            var lead   = new InventoryItem(BuiltinBlocks.GalenaLead, 50);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {aether, copper, iron, tin, gold, silver, lead},
                                    new InventoryItem(Item.ItemIndex, 1),
                                    10);

            ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes,
            GameLoader.NAMESPACE + ".Items.Elementium.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Elementium";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "Elementium.png");
            node["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("ingredient"));
            categories.AddToArray(new JSONNode("magic"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }
    }
}