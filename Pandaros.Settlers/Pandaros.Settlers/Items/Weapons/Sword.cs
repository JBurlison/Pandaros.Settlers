using BlockTypes;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items.Weapons
{
    [ModLoader.ModManager]
    public static class Sword
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,  GameLoader.NAMESPACE + ".Sword.RegisterRecipes")]
        public static void RegisterRecipes()
        {
            var coppertools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks      = new InventoryItem(BuiltinBlocks.Planks, 1);

            var copperParts = new InventoryItem(BuiltinBlocks.CopperParts, 5);
            var copper      = new InventoryItem(BuiltinBlocks.Copper, 5);

            var bronzePlate = new InventoryItem(BuiltinBlocks.BronzePlate, 5);
            var bronze      = new InventoryItem(BuiltinBlocks.BronzeIngot, 5);

            var ironRivet = new InventoryItem(BuiltinBlocks.IronRivet, 5);
            var iron      = new InventoryItem(BuiltinBlocks.IronWrought, 5);

            var steelParts = new InventoryItem(BuiltinBlocks.SteelParts, 5);
            var steel      = new InventoryItem(BuiltinBlocks.SteelIngot, 5);


            List<InventoryItem> items;

            foreach (var a in WeaponFactory.WeaponLookup)
            {
                items = new List<InventoryItem>();

                // ----------------------------------------
                // Copper
                // ----------------------------------------

                if (a.Value.Name == "Copper Sword")
                {
                    copperParts = new InventoryItem(BuiltinBlocks.CopperParts, 3);
                    copper      = new InventoryItem(BuiltinBlocks.Copper, 2);
                    items.AddRange(new[] {copper, copperParts, coppertools, planks});
                }

                // ----------------------------------------
                // Bronze
                // ----------------------------------------

                if (a.Value.Name == "Bronze Sword")
                {
                    bronzePlate = new InventoryItem(BuiltinBlocks.BronzePlate, 3);
                    bronze      = new InventoryItem(BuiltinBlocks.BronzeIngot, 2);
                    items.AddRange(new[] {bronze, bronzePlate, coppertools, planks});
                }

                // ----------------------------------------
                // Iron
                // ----------------------------------------

                if (a.Value.Name == "Iron Sword")
                {
                    ironRivet = new InventoryItem(BuiltinBlocks.IronRivet, 3);
                    iron      = new InventoryItem(BuiltinBlocks.IronIngot, 2);
                    items.AddRange(new[] {iron, ironRivet, coppertools, planks});
                }

                // ----------------------------------------
                // Steel
                // ----------------------------------------

                if (a.Value.Name == "Steel Sword")
                {
                    steelParts = new InventoryItem(BuiltinBlocks.SteelParts, 3);
                    steel      = new InventoryItem(BuiltinBlocks.SteelIngot, 2);
                    items.AddRange(new[] {steel, steelParts, coppertools, planks});
                }

                var invItem = new InventoryItem(a.Value.ItemType.ItemIndex);
                var recipe  = new Recipe(a.Value.ItemType.name, items, invItem, 5);

                ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(ItemFactory.JOB_METALSMITH, recipe);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Sword.AddSwords")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddSwords(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var copperSwordName = GameLoader.NAMESPACE + ".CopperSword";
            var copperSwordNode = new JSONNode();
            copperSwordNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "CopperSword.png");
            copperSwordNode["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("weapon"));
            copperSwordNode.SetAs("categories", categories);

            var copperSword = new ItemTypesServer.ItemTypeRaw(copperSwordName, copperSwordNode);
            items.Add(copperSwordName, copperSword);

            WeaponFactory.WeaponLookup.Add(copperSword.ItemIndex, new WeaponMetadata(50f, 50, "Copper Sword", copperSword));

            var bronzeSwordName = GameLoader.NAMESPACE + ".BronzeSword";
            var bronzeSwordNode = new JSONNode();
            bronzeSwordNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "BronzeSword.png");
            bronzeSwordNode["isPlaceable"] = new JSONNode(false);

            bronzeSwordNode.SetAs("categories", categories);

            var bronzeSword = new ItemTypesServer.ItemTypeRaw(bronzeSwordName, bronzeSwordNode);
            items.Add(bronzeSwordName, bronzeSword);

            WeaponFactory.WeaponLookup.Add(bronzeSword.ItemIndex, new WeaponMetadata(100f, 75, "Bronze Sword", bronzeSword));

            var IronSwordName = GameLoader.NAMESPACE + ".IronSword";
            var IronSwordNode = new JSONNode();
            IronSwordNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "IronSword.png");
            IronSwordNode["isPlaceable"] = new JSONNode(false);

            IronSwordNode.SetAs("categories", categories);

            var IronSword = new ItemTypesServer.ItemTypeRaw(IronSwordName, IronSwordNode);
            items.Add(IronSwordName, IronSword);

            WeaponFactory.WeaponLookup.Add(IronSword.ItemIndex, new WeaponMetadata(250f, 100, "Iron Sword", IronSword));

            var steelSwordName = GameLoader.NAMESPACE + ".SteelSword";
            var steelSwordNode = new JSONNode();
            steelSwordNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "SteelSword.png");
            steelSwordNode["isPlaceable"] = new JSONNode(false);

            steelSwordNode.SetAs("categories", categories);

            var steelSword = new ItemTypesServer.ItemTypeRaw(steelSwordName, steelSwordNode);
            items.Add(steelSwordName, steelSword);

            WeaponFactory.WeaponLookup.Add(steelSword.ItemIndex, new WeaponMetadata(500f, 150, "Steel Sword", steelSword));
        }
    }
}