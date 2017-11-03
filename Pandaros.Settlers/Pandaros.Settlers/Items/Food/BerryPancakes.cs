using BlockTypes.Builtin;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Food
{
    [ModLoader.ModManager]
    public static class BerryPancakes
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Food.RegisterBerryPancakes")]
        public static void RegisterBerryPancakes()
        {
            var flour = new InventoryItem(BuiltinBlocks.Flour, 3);
            var Berries = new InventoryItem(BuiltinBlocks.Berry, 2);
            var firewood = new InventoryItem("firewood");

            var invItem = new InventoryItem(Item.ItemIndex);
            var recipe = new Recipe(Item.name, new List<InventoryItem>() { flour, Berries, firewood }, invItem, 50);

            RecipeStorage.AddDefaultLimitTypeRecipe(ItemFactory.JOB_BAKER, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Food.AddBerryPancakes"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddBerryPie(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var foodName = GameLoader.NAMESPACE + ".BerryPancakes";
            var foodNode = new JSONNode();
            foodNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA.Replace("\\", "/") + "/BerryPancakes.png");
            foodNode["isPlaceable"] = new JSONNode(false);
            foodNode["NutritionalValue"] = new JSONNode(4);

            Item = new ItemTypesServer.ItemTypeRaw(foodName, foodNode);
            items.Add(foodName, Item);
        }
    }
}
