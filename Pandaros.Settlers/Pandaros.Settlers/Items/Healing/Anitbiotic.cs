using BlockTypes.Builtin;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Healing
{
    [ModLoader.ModManager]
    public static class Anitbiotic
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Healing.Anitbiotic.Register")]
        public static void Register()
        {
            var herbs = new InventoryItem(BuiltinBlocks.Hollyhock, 2);
            var herbs2 = new InventoryItem(BuiltinBlocks.Alkanet, 2);
            var oil = new InventoryItem(BuiltinBlocks.LinseedOil, 1);
            var flour = new InventoryItem(BuiltinBlocks.Flour, 2);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { flour, oil, herbs, herbs2 },
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Healing.Anitbiotic.Add"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Anitbiotic";
            var node = new JSONNode();
            node["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/Anitbiotic.png");
            node["isPlaceable"] = new JSONNode(false);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }


    }
}
