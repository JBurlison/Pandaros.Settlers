using System.Collections.Generic;
using Pandaros.Settlers.Jobs;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManagerAttribute]
    public static class WaterStone
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Items.WaterStone.Register")]
        public static void Register()
        {
            var aether = new InventoryItem(Elementium.Item.ItemIndex, 2);
            var torch  = new InventoryItem(Mana.Item.ItemIndex, 1);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {aether, torch},
                                    new InventoryItem(Item.ItemIndex, 1),
                                    6);

            RecipeStorage.AddOptionalLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterAddingBaseTypes,
            GameLoader.NAMESPACE + ".Items.WaterStone.Add")]
        [ModLoader.ModCallbackDependsOnAttribute("pipliz.blocknpcs.addlittypes")]
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