using BlockTypes;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items.Weapons
{
    [ModLoader.ModManager]
    public static class Sword
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,  GameLoader.NAMESPACE + ".Sword.RegisterRecipes")]
        public static void RegisterRecipes()
        {
            var coppertools = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1);
            var planks      = new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1);

            var copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 5);
            var copper      = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 5);

            var bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 5);
            var bronze      = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 5);

            var ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 5);
            var iron      = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 5);

            var steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 5);
            var steel      = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 5);


            List<InventoryItem> items;

            foreach (var a in WeaponFactory.WeaponLookup.Where(wepKvp => wepKvp.Value is WeaponMetadata weaponMetadata))
            {
                items = new List<InventoryItem>();

                // ----------------------------------------
                // Copper
                // ----------------------------------------

                if (a.Value.name == "Copper Sword")
                {
                    copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 3);
                    copper      = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 2);
                    items.AddRange(new[] {copper, copperParts, coppertools, planks});
                }

                // ----------------------------------------
                // Bronze
                // ----------------------------------------

                if (a.Value.name == "Bronze Sword")
                {
                    bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 3);
                    bronze      = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2);
                    items.AddRange(new[] {bronze, bronzePlate, coppertools, planks});
                }

                // ----------------------------------------
                // Iron
                // ----------------------------------------

                if (a.Value.name == "Iron Sword")
                {
                    ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 3);
                    iron      = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Name, 2);
                    items.AddRange(new[] {iron, ironRivet, coppertools, planks});
                }

                // ----------------------------------------
                // Steel
                // ----------------------------------------

                if (a.Value.name == "Steel Sword")
                {
                    steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 3);
                    steel      = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 2);
                    items.AddRange(new[] {steel, steelParts, coppertools, planks});
                }

                var metadata = a.Value as WeaponMetadata;
                var invItem = new RecipeResult(metadata.ItemType.ItemIndex);
                var recipe  = new Recipe(metadata.ItemType.name, items, invItem, 5);

                ServerManager.RecipeStorage.AddLimitTypeRecipe(ColonyBuiltIn.NpcTypes.METALSMITHJOB, recipe);
                ServerManager.RecipeStorage.AddScienceRequirement(recipe);
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

            WeaponFactory.WeaponLookup.Add(copperSword.ItemIndex, new WeaponMetadata(50f, 50, copperSwordName, copperSword));

            var bronzeSwordName = GameLoader.NAMESPACE + ".BronzeSword";
            var bronzeSwordNode = new JSONNode();
            bronzeSwordNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "BronzeSword.png");
            bronzeSwordNode["isPlaceable"] = new JSONNode(false);

            bronzeSwordNode.SetAs("categories", categories);

            var bronzeSword = new ItemTypesServer.ItemTypeRaw(bronzeSwordName, bronzeSwordNode);
            items.Add(bronzeSwordName, bronzeSword);

            WeaponFactory.WeaponLookup.Add(bronzeSword.ItemIndex, new WeaponMetadata(100f, 75, bronzeSwordName, bronzeSword));

            var IronSwordName = GameLoader.NAMESPACE + ".IronSword";
            var IronSwordNode = new JSONNode();
            IronSwordNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "IronSword.png");
            IronSwordNode["isPlaceable"] = new JSONNode(false);

            IronSwordNode.SetAs("categories", categories);

            var IronSword = new ItemTypesServer.ItemTypeRaw(IronSwordName, IronSwordNode);
            items.Add(IronSwordName, IronSword);

            WeaponFactory.WeaponLookup.Add(IronSword.ItemIndex, new WeaponMetadata(250f, 100, IronSwordName, IronSword));

            var steelSwordName = GameLoader.NAMESPACE + ".SteelSword";
            var steelSwordNode = new JSONNode();
            steelSwordNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "SteelSword.png");
            steelSwordNode["isPlaceable"] = new JSONNode(false);

            steelSwordNode.SetAs("categories", categories);

            var steelSword = new ItemTypesServer.ItemTypeRaw(steelSwordName, steelSwordNode);
            items.Add(steelSwordName, steelSword);

            WeaponFactory.WeaponLookup.Add(steelSword.ItemIndex, new WeaponMetadata(500f, 150, steelSwordName, steelSword));
        }
    }
}