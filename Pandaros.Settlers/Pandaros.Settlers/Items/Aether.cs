using BlockTypes;
using Pandaros.Settlers.Jobs;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class Aether
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Aether.Register")]
        public static void Register()
        {
            var mana     = new InventoryItem(Mana.Item.ItemIndex, 2);
            var cryastal = new InventoryItem(BuiltinBlocks.Crystal, 10);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {mana, cryastal},
                                    new InventoryItem(Item.ItemIndex, 1),
                                    20);

            ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Aether.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Aether";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "redbottle.png");
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