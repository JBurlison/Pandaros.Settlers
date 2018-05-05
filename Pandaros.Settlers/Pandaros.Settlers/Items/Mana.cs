using BlockTypes.Builtin;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class Mana
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Mana.Register")]
        public static void Register()
        {
            var herbs = new InventoryItem(BuiltinBlocks.Hollyhock, 10);
            var herbs2 = new InventoryItem(BuiltinBlocks.Alkanet, 10);
            var oil = new InventoryItem(BuiltinBlocks.LinseedOil, 10);
            var herbs3 = new InventoryItem(BuiltinBlocks.Wolfsbane, 10);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { herbs3, oil, herbs, herbs2 },
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Mana.Add"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Mana";
            var node = new JSONNode();
            node["icon"] = new JSONNode("bluebottle.png");
            node["isPlaceable"] = new JSONNode(false);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("ingredient"));
            categories.AddToArray(new JSONNode("magic"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }
    }
}
