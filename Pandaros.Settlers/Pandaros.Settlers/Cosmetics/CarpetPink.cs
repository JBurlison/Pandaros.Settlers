using BlockTypes.Builtin;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Cosmetics
{
    [ModLoader.ModManager]
    public static class CarpetPink
    {
        const string KEY = "carpetpink";
        
        public static string TEXTURE_KEY = GameLoader.NAMESPACE + "." + KEY;
        public static string NAME = GameLoader.NAMESPACE + "." + KEY;

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Cosmetics." + KEY + ".AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            Register.AddCarpetTextures(KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Cosmetics." + KEY + ".AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            Item = Register.AddCarpetTypeTypes(itemTypes, KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Cosmetics." + KEY + ".AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var flax = new InventoryItem(BuiltinBlocks.Flax, 1);
            var planks = new InventoryItem(BuiltinBlocks.Planks, 1);
            var linen = new InventoryItem(BuiltinBlocks.Linen, 1);

            var recipe = new Recipe(NAME,
                    new List<InventoryItem>() { flax, planks, linen },
                    new InventoryItem(Item.ItemIndex, 1), 2);

            ItemTypesServer.LoadSortOrder(NAME, GameLoader.GetNextItemSortIndex());
            RecipeStorage.AddDefaultLimitTypeRecipe(Register.DYER_JOB, recipe);
        }
    }
}
