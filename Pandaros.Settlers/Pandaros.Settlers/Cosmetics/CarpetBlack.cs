using System.Collections.Generic;
using BlockTypes.Builtin;

namespace Pandaros.Settlers.Cosmetics
{
    [ModLoader.ModManagerAttribute]
    public static class CarpetBlack
    {
        private const string KEY = "carpetblack";
        public static string NAME = GameLoader.NAMESPACE + "." + KEY;

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld,
            GameLoader.NAMESPACE + ".Cosmetics." + KEY + ".AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            Register.AddCarpetTextures(KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes,
            GameLoader.NAMESPACE + ".Cosmetics." + KEY + ".AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            Item = Register.AddCarpetTypeTypes(itemTypes, KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Cosmetics." + KEY + ".AfterItemTypesDefined")]
        public static void AfterItemTypesDefined()
        {
            var flax   = new InventoryItem(BuiltinBlocks.Flax, 1);
            var planks = new InventoryItem(BuiltinBlocks.Planks, 1);
            var linen  = new InventoryItem(BuiltinBlocks.Linen, 1);

            var recipe = new Recipe(NAME,
                                    new List<InventoryItem> {flax, planks, linen},
                                    new InventoryItem(Item.ItemIndex, 1), 2);

            RecipeStorage.AddDefaultLimitTypeRecipe(Register.DYER_JOB, recipe);
        }
    }
}