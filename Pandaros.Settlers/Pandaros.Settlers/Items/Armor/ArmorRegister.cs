using Pandaros.API;
using Pandaros.API.Items.Armor;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items.Armor
{
    [ModLoader.ModManager]
    public static class ArmorRegister
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Armor.RegisterRecipes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void RegisterRecipes()
        {
            var coppertools = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1);
            var clothing = new InventoryItem(ColonyBuiltIn.ItemTypes.LINEN.Name, 1);

            var copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 5);
            var copper = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 5);

            var bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 5);
            var bronze = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 5);

            var ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 5);
            var iron = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 5);

            var steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 5);
            var steel = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 5);

            List<InventoryItem> items;

            foreach (var a in ArmorFactory.ArmorLookup.Where(a => a.Value is ArmorMetadata metadata))
            {
                items = new List<InventoryItem>();

                // ----------------------------------------
                // Copper
                // ----------------------------------------

                if (a.Value.name.Contains("Copper") && a.Value.Slot == ArmorFactory.ArmorSlot.Helm)
                {
                    copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 3);
                    copper = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 2);
                    items.AddRange(new[] { copper, copperParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Copper") && a.Value.Slot == ArmorFactory.ArmorSlot.Chest)
                {
                    copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 5);
                    copper = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 5);
                    items.AddRange(new[] { copper, copperParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Copper") && a.Value.Slot == ArmorFactory.ArmorSlot.Gloves)
                {
                    copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 2);
                    copper = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 2);
                    items.AddRange(new[] { copper, copperParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Copper") && a.Value.Slot == ArmorFactory.ArmorSlot.Legs)
                {
                    copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 3);
                    copper = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 3);
                    items.AddRange(new[] { copper, copperParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Copper") && a.Value.Slot == ArmorFactory.ArmorSlot.Boots)
                {
                    copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 2);
                    copper = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 2);
                    items.AddRange(new[] { copper, copperParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Copper") && a.Value.Slot == ArmorFactory.ArmorSlot.Shield)
                {
                    copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 2);
                    copper = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 2);
                    items.AddRange(new[] { copper, copperParts, coppertools, clothing });
                }

                // ----------------------------------------
                // Bronze
                // ----------------------------------------

                if (a.Value.name.Contains("Bronze") && a.Value.Slot == ArmorFactory.ArmorSlot.Helm)
                {
                    bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 3);
                    bronze = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2);
                    items.AddRange(new[] { bronze, bronzePlate, coppertools, clothing });
                }

                if (a.Value.name.Contains("Bronze") && a.Value.Slot == ArmorFactory.ArmorSlot.Chest)
                {
                    bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 5);
                    bronze = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 5);
                    items.AddRange(new[] { bronze, bronzePlate, coppertools, clothing });
                }

                if (a.Value.name.Contains("Bronze") && a.Value.Slot == ArmorFactory.ArmorSlot.Gloves)
                {
                    bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 2);
                    bronze = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2);
                    items.AddRange(new[] { bronze, bronzePlate, coppertools, clothing });
                }

                if (a.Value.name.Contains("Bronze") && a.Value.Slot == ArmorFactory.ArmorSlot.Legs)
                {
                    bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 3);
                    bronze = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 3);
                    items.AddRange(new[] { bronze, bronzePlate, coppertools, clothing });
                }

                if (a.Value.name.Contains("Bronze") && a.Value.Slot == ArmorFactory.ArmorSlot.Boots)
                {
                    bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 2);
                    bronze = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2);
                    items.AddRange(new[] { bronze, bronzePlate, coppertools, clothing });
                }

                if (a.Value.name.Contains("Bronze") && a.Value.Slot == ArmorFactory.ArmorSlot.Shield)
                {
                    bronzePlate = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 2);
                    bronze = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2);
                    items.AddRange(new[] { bronze, bronzePlate, coppertools, clothing });
                }

                // ----------------------------------------
                // Iron
                // ----------------------------------------

                if (a.Value.name.Contains("Iron") && a.Value.Slot == ArmorFactory.ArmorSlot.Helm)
                {
                    ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 3);
                    iron = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Name, 2);
                    items.AddRange(new[] { iron, ironRivet, coppertools, clothing });
                }

                if (a.Value.name.Contains("Iron") && a.Value.Slot == ArmorFactory.ArmorSlot.Chest)
                {
                    ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 5);
                    iron = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Name, 5);
                    items.AddRange(new[] { iron, ironRivet, coppertools, clothing });
                }

                if (a.Value.name.Contains("Iron") && a.Value.Slot == ArmorFactory.ArmorSlot.Gloves)
                {
                    ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 2);
                    iron = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Name, 2);
                    items.AddRange(new[] { iron, ironRivet, coppertools, clothing });
                }

                if (a.Value.name.Contains("Iron") && a.Value.Slot == ArmorFactory.ArmorSlot.Legs)
                {
                    ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 3);
                    iron = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Name, 3);
                    items.AddRange(new[] { iron, ironRivet, coppertools, clothing });
                }

                if (a.Value.name.Contains("Iron") && a.Value.Slot == ArmorFactory.ArmorSlot.Boots)
                {
                    ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 2);
                    iron = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Name, 2);
                    items.AddRange(new[] { iron, ironRivet, coppertools, clothing });
                }

                if (a.Value.name.Contains("Iron") && a.Value.Slot == ArmorFactory.ArmorSlot.Shield)
                {
                    ironRivet = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 2);
                    iron = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Name, 2);
                    items.AddRange(new[] { iron, ironRivet, coppertools, clothing });
                }

                // ----------------------------------------
                // Steel
                // ----------------------------------------

                if (a.Value.name.Contains("Steel") && a.Value.Slot == ArmorFactory.ArmorSlot.Helm)
                {
                    steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 3);
                    steel = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 2);
                    items.AddRange(new[] { steel, steelParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Steel") && a.Value.Slot == ArmorFactory.ArmorSlot.Chest)
                {
                    steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 5);
                    steel = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 5);
                    items.AddRange(new[] { steel, steelParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Steel") && a.Value.Slot == ArmorFactory.ArmorSlot.Gloves)
                {
                    steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 2);
                    steel = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 2);
                    items.AddRange(new[] { steel, steelParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Steel") && a.Value.Slot == ArmorFactory.ArmorSlot.Legs)
                {
                    steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 3);
                    steel = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 3);
                    items.AddRange(new[] { steel, steelParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Steel") && a.Value.Slot == ArmorFactory.ArmorSlot.Boots)
                {
                    steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 2);
                    steel = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 2);
                    items.AddRange(new[] { steel, steelParts, coppertools, clothing });
                }

                if (a.Value.name.Contains("Steel") && a.Value.Slot == ArmorFactory.ArmorSlot.Shield)
                {
                    steelParts = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 2);
                    steel = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 2);
                    items.AddRange(new[] { steel, steelParts, coppertools, clothing });
                }


                var metaData = (ArmorMetadata)a.Value; 
                var invItem = new RecipeResult(metaData.ItemType.ItemIndex);
                var recipe = new Recipe(metaData.ItemType.name, items, invItem, 5, 0, -100);

                ServerManager.RecipeStorage.AddLimitTypeRecipe(ColonyBuiltIn.NpcTypes.METALSMITHJOB, recipe);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Armor.AddArmor")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public static void AddArmor(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            try
            {
                // ----------------------------------------
                // Copper
                // ----------------------------------------

                // Helm
                var copperHelmName = GameLoader.NAMESPACE + ".CopperHelm";
                var copperHelmNode = new JSONNode();
                copperHelmNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "CopperHelm.png");
                copperHelmNode["isPlaceable"] = new JSONNode(false);

                var categories = new JSONNode(NodeType.Array);
                categories.AddToArray(new JSONNode("armor"));
                copperHelmNode.SetAs("categories", categories);

                var copperHelm = new ItemTypesServer.ItemTypeRaw(copperHelmName, copperHelmNode);
                items.Add(copperHelmName, copperHelm);

                ArmorFactory.ArmorLookup.Add(copperHelm.ItemIndex,
                                new ArmorMetadata(0.05f, 15, copperHelmName, copperHelm, ArmorFactory.ArmorSlot.Helm));

                // Chest
                var copperChestName = GameLoader.NAMESPACE + ".CopperChest";
                var copperChestNode = new JSONNode();
                copperChestNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "CopperChest.png");
                copperChestNode["isPlaceable"] = new JSONNode(false);

                copperChestNode.SetAs("categories", categories);

                var copperChest = new ItemTypesServer.ItemTypeRaw(copperChestName, copperChestNode);
                items.Add(copperChestName, copperChest);

                ArmorFactory.ArmorLookup.Add(copperChest.ItemIndex,
                                new ArmorMetadata(.1f, 25, copperChestName, copperChest, ArmorFactory.ArmorSlot.Chest));

                // Gloves
                var copperGlovesName = GameLoader.NAMESPACE + ".CopperGloves";
                var copperGlovesNode = new JSONNode();
                copperGlovesNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "CopperGloves.png");
                copperGlovesNode["isPlaceable"] = new JSONNode(false);

                copperGlovesNode.SetAs("categories", categories);

                var copperGloves = new ItemTypesServer.ItemTypeRaw(copperGlovesName, copperGlovesNode);
                items.Add(copperGlovesName, copperGloves);

                ArmorFactory.ArmorLookup.Add(copperGloves.ItemIndex,
                                new ArmorMetadata(0.025f, 10, copperGlovesName, copperGloves, ArmorFactory.ArmorSlot.Gloves));

                // Legs
                var copperLegsName = GameLoader.NAMESPACE + ".CopperLegs";
                var copperLegsNode = new JSONNode();
                copperLegsNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "CopperLegs.png");
                copperLegsNode["isPlaceable"] = new JSONNode(false);

                copperLegsNode.SetAs("categories", categories);

                var copperLegs = new ItemTypesServer.ItemTypeRaw(copperLegsName, copperLegsNode);
                items.Add(copperLegsName, copperLegs);

                ArmorFactory.ArmorLookup.Add(copperLegs.ItemIndex,
                                new ArmorMetadata(0.07f, 20, copperLegsName, copperLegs, ArmorFactory.ArmorSlot.Legs));

                // Boots
                var copperBootsName = GameLoader.NAMESPACE + ".CopperBoots";
                var copperBootsNode = new JSONNode();
                copperBootsNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "CopperBoots.png");
                copperBootsNode["isPlaceable"] = new JSONNode(false);

                copperBootsNode.SetAs("categories", categories);

                var copperBoots = new ItemTypesServer.ItemTypeRaw(copperBootsName, copperBootsNode);
                items.Add(copperBootsName, copperBoots);

                ArmorFactory.ArmorLookup.Add(copperBoots.ItemIndex,
                                new ArmorMetadata(0.025f, 10, copperBootsName, copperBoots, ArmorFactory.ArmorSlot.Boots));

                // Shield
                var copperShieldName = GameLoader.NAMESPACE + ".CopperShield";
                var copperShieldNode = new JSONNode();
                copperShieldNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "CopperShield.png");
                copperShieldNode["isPlaceable"] = new JSONNode(false);

                copperShieldNode.SetAs("categories", categories);

                var copperShield = new ItemTypesServer.ItemTypeRaw(copperShieldName, copperShieldNode);
                items.Add(copperShieldName, copperShield);

                ArmorFactory.ArmorLookup.Add(copperShield.ItemIndex,
                                new ArmorMetadata(0.05f, 30, copperShieldName, copperShield, ArmorFactory.ArmorSlot.Shield));

                // ----------------------------------------
                // Bronze
                // ----------------------------------------

                // Helm
                var bronzeHelmName = GameLoader.NAMESPACE + ".BronzeHelm";
                var bronzeHelmNode = new JSONNode();
                bronzeHelmNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "BronzeHelm.png");
                bronzeHelmNode["isPlaceable"] = new JSONNode(false);

                bronzeHelmNode.SetAs("categories", categories);

                var bronzeHelm = new ItemTypesServer.ItemTypeRaw(bronzeHelmName, bronzeHelmNode);
                items.Add(bronzeHelmName, bronzeHelm);

                ArmorFactory.ArmorLookup.Add(bronzeHelm.ItemIndex,
                                new ArmorMetadata(0.07f, 20, bronzeHelmName, bronzeHelm, ArmorFactory.ArmorSlot.Helm));

                // Chest
                var bronzeChestName = GameLoader.NAMESPACE + ".BronzeChest";
                var bronzeChestNode = new JSONNode();
                bronzeChestNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "BronzeChest.png");
                bronzeChestNode["isPlaceable"] = new JSONNode(false);

                bronzeChestNode.SetAs("categories", categories);

                var bronzeChest = new ItemTypesServer.ItemTypeRaw(bronzeChestName, bronzeChestNode);
                items.Add(bronzeChestName, bronzeChest);

                ArmorFactory.ArmorLookup.Add(bronzeChest.ItemIndex,
                                new ArmorMetadata(.15f, 30, bronzeChestName, bronzeChest, ArmorFactory.ArmorSlot.Chest));

                // Gloves
                var bronzeGlovesName = GameLoader.NAMESPACE + ".BronzeGloves";
                var bronzeGlovesNode = new JSONNode();
                bronzeGlovesNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "BronzeGloves.png");
                bronzeGlovesNode["isPlaceable"] = new JSONNode(false);

                bronzeGlovesNode.SetAs("categories", categories);

                var bronzeGloves = new ItemTypesServer.ItemTypeRaw(bronzeGlovesName, bronzeGlovesNode);
                items.Add(bronzeGlovesName, bronzeGloves);

                ArmorFactory.ArmorLookup.Add(bronzeGloves.ItemIndex,
                                new ArmorMetadata(0.04f, 15, bronzeGlovesName, bronzeGloves, ArmorFactory.ArmorSlot.Gloves));

                // Legs
                var bronzeLegsName = GameLoader.NAMESPACE + ".BronzeLegs";
                var bronzeLegsNode = new JSONNode();
                bronzeLegsNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "BronzeLegs.png");
                bronzeLegsNode["isPlaceable"] = new JSONNode(false);

                bronzeLegsNode.SetAs("categories", categories);

                var bronzeLegs = new ItemTypesServer.ItemTypeRaw(bronzeLegsName, bronzeLegsNode);
                items.Add(bronzeLegsName, bronzeLegs);

                ArmorFactory.ArmorLookup.Add(bronzeLegs.ItemIndex,
                                new ArmorMetadata(0.09f, 25, bronzeLegsName, bronzeLegs, ArmorFactory.ArmorSlot.Legs));

                // Boots
                var bronzeBootsName = GameLoader.NAMESPACE + ".BronzeBoots";
                var bronzeBootsNode = new JSONNode();
                bronzeBootsNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "BronzeBoots.png");
                bronzeBootsNode["isPlaceable"] = new JSONNode(false);

                bronzeBootsNode.SetAs("categories", categories);

                var bronzeBoots = new ItemTypesServer.ItemTypeRaw(bronzeBootsName, bronzeBootsNode);
                items.Add(bronzeBootsName, bronzeBoots);

                ArmorFactory.ArmorLookup.Add(bronzeBoots.ItemIndex,
                                new ArmorMetadata(0.04f, 15, bronzeBootsName, bronzeBoots, ArmorFactory.ArmorSlot.Boots));

                // Shield
                var bronzeShieldName = GameLoader.NAMESPACE + ".BronzeShield";
                var bronzeShieldNode = new JSONNode();
                bronzeShieldNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "BronzeShield.png");
                bronzeShieldNode["isPlaceable"] = new JSONNode(false);

                bronzeShieldNode.SetAs("categories", categories);

                var bronzeShield = new ItemTypesServer.ItemTypeRaw(bronzeShieldName, bronzeShieldNode);
                items.Add(bronzeShieldName, bronzeShield);

                ArmorFactory.ArmorLookup.Add(bronzeShield.ItemIndex,
                                new ArmorMetadata(0.07f, 40, bronzeShieldName, bronzeShield, ArmorFactory.ArmorSlot.Shield));

                // ----------------------------------------
                // Iron
                // ----------------------------------------

                // Helm
                var ironHelmName = GameLoader.NAMESPACE + ".IronHelm";
                var ironHelmNode = new JSONNode();
                ironHelmNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "IronHelm.png");
                ironHelmNode["isPlaceable"] = new JSONNode(false);

                ironHelmNode.SetAs("categories", categories);

                var ironHelm = new ItemTypesServer.ItemTypeRaw(ironHelmName, ironHelmNode);
                items.Add(ironHelmName, ironHelm);

                ArmorFactory.ArmorLookup.Add(ironHelm.ItemIndex,
                                new ArmorMetadata(0.09f, 30, ironHelmName, ironHelm, ArmorFactory.ArmorSlot.Helm));

                // Chest
                var ironChestName = GameLoader.NAMESPACE + ".IronChest";
                var ironChestNode = new JSONNode();
                ironChestNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "IronChest.png");
                ironChestNode["isPlaceable"] = new JSONNode(false);

                ironChestNode.SetAs("categories", categories);

                var ironChest = new ItemTypesServer.ItemTypeRaw(ironChestName, ironChestNode);
                items.Add(ironChestName, ironChest);

                ArmorFactory.ArmorLookup.Add(ironChest.ItemIndex,
                                new ArmorMetadata(.2f, 40, ironChestName, ironChest, ArmorFactory.ArmorSlot.Chest));

                // Gloves
                var ironGlovesName = GameLoader.NAMESPACE + ".IronGloves";
                var ironGlovesNode = new JSONNode();
                ironGlovesNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "IronGloves.png");
                ironGlovesNode["isPlaceable"] = new JSONNode(false);

                ironGlovesNode.SetAs("categories", categories);

                var ironGloves = new ItemTypesServer.ItemTypeRaw(ironGlovesName, ironGlovesNode);
                items.Add(ironGlovesName, ironGloves);

                ArmorFactory.ArmorLookup.Add(ironGloves.ItemIndex,
                                new ArmorMetadata(0.055f, 25, ironGlovesName, ironGloves, ArmorFactory.ArmorSlot.Gloves));

                // Legs
                var ironLegsName = GameLoader.NAMESPACE + ".IronLegs";
                var ironLegsNode = new JSONNode();
                ironLegsNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "IronLegs.png");
                ironLegsNode["isPlaceable"] = new JSONNode(false);

                ironLegsNode.SetAs("categories", categories);

                var ironLegs = new ItemTypesServer.ItemTypeRaw(ironLegsName, ironLegsNode);
                items.Add(ironLegsName, ironLegs);

                ArmorFactory.ArmorLookup.Add(ironLegs.ItemIndex,
                                new ArmorMetadata(0.11f, 35, ironLegsName, ironLegs, ArmorFactory.ArmorSlot.Legs));

                // Boots
                var ironBootsName = GameLoader.NAMESPACE + ".IronBoots";
                var ironBootsNode = new JSONNode();
                ironBootsNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "IronBoots.png");
                ironBootsNode["isPlaceable"] = new JSONNode(false);

                ironBootsNode.SetAs("categories", categories);

                var ironBoots = new ItemTypesServer.ItemTypeRaw(ironBootsName, ironBootsNode);
                items.Add(ironBootsName, ironBoots);

                ArmorFactory.ArmorLookup.Add(ironBoots.ItemIndex,
                                new ArmorMetadata(0.055f, 25, ironBootsName, ironBoots, ArmorFactory.ArmorSlot.Boots));

                // Shield
                var ironShieldName = GameLoader.NAMESPACE + ".IronShield";
                var ironShieldNode = new JSONNode();
                ironShieldNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "IronShield.png");
                ironShieldNode["isPlaceable"] = new JSONNode(false);

                ironShieldNode.SetAs("categories", categories);

                var ironShield = new ItemTypesServer.ItemTypeRaw(ironShieldName, ironShieldNode);
                items.Add(ironShieldName, ironShield);

                ArmorFactory.ArmorLookup.Add(ironShield.ItemIndex,
                                new ArmorMetadata(0.1f, 50, ironShieldName, ironShield, ArmorFactory.ArmorSlot.Shield));

                // ----------------------------------------
                // Steel
                // ----------------------------------------

                // Helm
                var steelHelmName = GameLoader.NAMESPACE + ".SteelHelm";
                var steelHelmNode = new JSONNode();
                steelHelmNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "SteelHelm.png");
                steelHelmNode["isPlaceable"] = new JSONNode(false);

                steelHelmNode.SetAs("categories", categories);

                var steelHelm = new ItemTypesServer.ItemTypeRaw(steelHelmName, steelHelmNode);
                items.Add(steelHelmName, steelHelm);

                ArmorFactory.ArmorLookup.Add(steelHelm.ItemIndex,
                                new ArmorMetadata(0.11f, 40, steelHelmName, steelHelm, ArmorFactory.ArmorSlot.Helm));

                // Chest
                var steelChestName = GameLoader.NAMESPACE + ".SteelChest";
                var steelChestNode = new JSONNode();
                steelChestNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "SteelChest.png");
                steelChestNode["isPlaceable"] = new JSONNode(false);

                steelChestNode.SetAs("categories", categories);

                var steelChest = new ItemTypesServer.ItemTypeRaw(steelChestName, steelChestNode);
                items.Add(steelChestName, steelChest);

                ArmorFactory.ArmorLookup.Add(steelChest.ItemIndex,
                                new ArmorMetadata(.3f, 50, steelChestName, steelChest, ArmorFactory.ArmorSlot.Chest));

                // Gloves
                var steelGlovesName = GameLoader.NAMESPACE + ".SteelGloves";
                var steelGlovesNode = new JSONNode();
                steelGlovesNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "SteelGloves.png");
                steelGlovesNode["isPlaceable"] = new JSONNode(false);

                steelGlovesNode.SetAs("categories", categories);

                var steelGloves = new ItemTypesServer.ItemTypeRaw(steelGlovesName, steelGlovesNode);
                items.Add(steelGlovesName, steelGloves);

                ArmorFactory.ArmorLookup.Add(steelGloves.ItemIndex,
                                new ArmorMetadata(0.07f, 35, steelGlovesName, steelGloves, ArmorFactory.ArmorSlot.Gloves));

                // Legs
                var steelLegsName = GameLoader.NAMESPACE + ".SteelLegs";
                var steelLegsNode = new JSONNode();
                steelLegsNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "SteelLegs.png");
                steelLegsNode["isPlaceable"] = new JSONNode(false);

                steelLegsNode.SetAs("categories", categories);

                var steelLegs = new ItemTypesServer.ItemTypeRaw(steelLegsName, steelLegsNode);
                items.Add(steelLegsName, steelLegs);

                ArmorFactory.ArmorLookup.Add(steelLegs.ItemIndex,
                                new ArmorMetadata(0.13f, 40, steelLegsName, steelLegs, ArmorFactory.ArmorSlot.Legs));

                // Boots
                var steelBootsName = GameLoader.NAMESPACE + ".SteelBoots";
                var steelBootsNode = new JSONNode();
                steelBootsNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "SteelBoots.png");
                steelBootsNode["isPlaceable"] = new JSONNode(false);

                steelBootsNode.SetAs("categories", categories);

                var steelBoots = new ItemTypesServer.ItemTypeRaw(steelBootsName, steelBootsNode);
                items.Add(steelBootsName, steelBoots);

                ArmorFactory.ArmorLookup.Add(steelBoots.ItemIndex,
                                new ArmorMetadata(0.07f, 35, steelBootsName, steelBoots, ArmorFactory.ArmorSlot.Boots));

                // Shield
                var steelShieldName = GameLoader.NAMESPACE + ".SteelShield";
                var steelShieldNode = new JSONNode();
                steelShieldNode["icon"] = new JSONNode(GameLoader.ICON_PATH + "SteelShield.png");
                steelShieldNode["isPlaceable"] = new JSONNode(false);

                steelShieldNode.SetAs("categories", categories);

                var steelShield = new ItemTypesServer.ItemTypeRaw(steelShieldName, steelShieldNode);
                items.Add(steelShieldName, steelShield);

                ArmorFactory.ArmorLookup.Add(steelShield.ItemIndex,
                                new ArmorMetadata(0.12f, 60, steelShieldName, steelShield, ArmorFactory.ArmorSlot.Shield));

                ArmorFactory.ArmorLookup = ArmorFactory.ArmorLookup.OrderBy(kvp => kvp.Value.name).ThenBy(kvp => kvp.Value.ArmorRating)
                                         .ToDictionary(k => k.Key, v => v.Value);
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }
        }
    }
}