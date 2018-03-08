using BlockTypes.Builtin;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class AirStone
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.AirStone.Register")]
        public static void Register()
        {
            var aether = new InventoryItem(Aether.Item.ItemIndex, 1);
            var torch = new InventoryItem(BuiltinBlocks.BronzeArrow, 20);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { aether, torch },
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);

            //ItemTypesServer.LoadSortOrder(Item.name, GameLoader.GetNextItemSortIndex());
            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.AirStone.Add"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".AirStone";
            var node = new JSONNode();
            node["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/Airstone.png");
            node["isPlaceable"] = new JSONNode(false);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }
    }
}
