using BlockTypes.Builtin;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class Elementium
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Elementium.Register")]
        public static void Register()
        {
            var aether = new InventoryItem(Aether.Item.ItemIndex, 1);
            var copper = new InventoryItem(BuiltinBlocks.Copper, 20);
            var iron = new InventoryItem(BuiltinBlocks.IronOre, 20);
            var tin = new InventoryItem(BuiltinBlocks.Tin, 20);
            var gold = new InventoryItem(BuiltinBlocks.GoldOre, 20);
            var silver = new InventoryItem(BuiltinBlocks.GalenaSilver, 20);
            var lead = new InventoryItem(BuiltinBlocks.GalenaLead, 20);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { aether, copper, iron, tin, gold, silver, lead },
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);

            //ItemTypesServer.LoadSortOrder(Item.name, GameLoader.GetNextItemSortIndex());
            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Elementium.Add"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Elementium";
            var node = new JSONNode();
            node["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/Elementium.png");
            node["isPlaceable"] = new JSONNode(false);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }
    }
}
