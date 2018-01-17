using BlockTypes.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items.Healing
{
    //[ModLoader.ModManager]
    public static class Antidote
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Healing.Antidote.Register")]
        public static void Register()
        {
            var herbs = new InventoryItem(BuiltinBlocks.Wolfsbane, 4);
            var oil = new InventoryItem(BuiltinBlocks.LinseedOil, 1);
            var Berries = new InventoryItem(BuiltinBlocks.Berry, 4);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { herbs, Berries, oil },
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);

            RecipeStorage.AddDefaultLimitTypeRecipe(Jobs.ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Healing.Antidote.Add"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Antidote";
            var node = new JSONNode();
            node["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/greenbottle.png");
            node["isPlaceable"] = new JSONNode(false);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }
    }
}
