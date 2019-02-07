using BlockTypes;
using Pandaros.Settlers.Jobs;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class Mana
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Mana.Register")]
        public static void Register()
        {
            var herbs  = new InventoryItem(ColonyBuiltIn.ItemTypes.HOLLYHOCK.Name, 10);
            var herbs2 = new InventoryItem(ColonyBuiltIn.ItemTypes.ALKANET.Name, 10);
            var oil    = new InventoryItem(ColonyBuiltIn.ItemTypes.OLIVEOIL.Name, 10);
            var herbs3 = new InventoryItem(ColonyBuiltIn.ItemTypes.WOLFSBANE.Name, 10);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {herbs3, oil, herbs, herbs2},
                                    new ItemTypes.ItemTypeDrops(Item.ItemIndex, 1),
                                    50);

            recipe.Results.Add(new ItemTypes.ItemTypeDrops(Esper.Item.ItemIndex, 1, 0.03f));
            ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Items.Mana.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Mana";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "bluebottle.png");
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