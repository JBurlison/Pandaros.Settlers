using System.Collections.Generic;
using BlockTypes.Builtin;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items.Food
{
    [ModLoader.ModManagerAttribute]
    public static class BerryPancakes
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Items.Food.RegisterBerryPancakes")]
        public static void RegisterBerryPancakes()
        {
            var flour    = new InventoryItem(BuiltinBlocks.Flour, 3);
            var Berries  = new InventoryItem(BuiltinBlocks.Berry, 2);
            var firewood = new InventoryItem(BuiltinBlocks.Firewood);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {flour, Berries, firewood},
                                    new InventoryItem(Item.ItemIndex, 2),
                                    50);

            RecipeStorage.AddDefaultLimitTypeRecipe(ItemFactory.JOB_BAKER, recipe);
        }


        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterAddingBaseTypes,
            GameLoader.NAMESPACE + ".Items.Food.AddBerryPancakes")]
        [ModLoader.ModCallbackDependsOnAttribute("pipliz.blocknpcs.addlittypes")]
        public static void AddBerryPie(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var foodName = GameLoader.NAMESPACE + ".BerryPancakes";
            var foodNode = new JSONNode();
            foodNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "BerryPancakes.png");
            foodNode["isPlaceable"] = new JSONNode(false);
            foodNode.SetAs("nutritionalValue", 4f);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("food"));
            foodNode.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(foodName, foodNode);
            items.Add(foodName, Item);
        }
    }
}