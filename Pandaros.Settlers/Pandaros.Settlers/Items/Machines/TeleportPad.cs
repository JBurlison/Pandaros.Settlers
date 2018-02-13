using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class TeleportPad
    {
        const double TeleportPadCooldown = 4;
        
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.RegisterMachines")]
        public static void RegisterMachines(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            MachineManager.RegisterMachineType(nameof(TeleportPad), new MachineManager.MachineSettings(Item.ItemIndex, Repair, MachineManager.Refuel, Reload, DoWork, 10, 4, 5, 10));
        }

        public static ushort Repair(Players.Player player, MachineState machineState)
        {
            var retval = GameLoader.Repairing_Icon;
            var ps = PlayerState.GetPlayerState(player);

            if (machineState.Durability < .75f)
            {
                bool repaired = false;
                List<InventoryItem> requiredForFix = new List<InventoryItem>();
                var stockpile = Stockpile.GetStockPile(player);

                requiredForFix.Add(new InventoryItem(BuiltinBlocks.StoneBricks, 5));
                requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagBasic, 2));

                if (machineState.Durability < .10f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagAdvanced, 2));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagColony, 2));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.SteelIngot, 2));
                }
                else if (machineState.Durability < .30f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagAdvanced, 1));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagColony, 2));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.SteelIngot, 2));
                }
                else if (machineState.Durability < .50f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagAdvanced, 1));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.SteelIngot, 1));
                }

                if (stockpile.Contains(requiredForFix))
                {
                    stockpile.TryRemove(requiredForFix);
                    repaired = true;
                }
                else
                    foreach (var item in requiredForFix)
                        if (!stockpile.Contains(item))
                        {
                            retval = item.Type;
                            break;
                        }

                if (!MachineState.MAX_DURABILITY.ContainsKey(player))
                    MachineState.MAX_DURABILITY[player] = MachineState.DEFAULT_MAX_DURABILITY;

                if (repaired)
                    machineState.Durability = MachineState.MAX_DURABILITY[player];
            }

            return retval;
        }
        
        public static ushort Reload(Players.Player player, MachineState machineState)
        {
            return GameLoader.Waiting_Icon;
        }

        public static ushort Refuel(Players.Player player, MachineState machineState)
        {
            var ps = PlayerState.GetPlayerState(player);

            if (machineState.Fuel < .75f)
            {
                if (!MachineState.MAX_FUEL.ContainsKey(player))
                    MachineState.MAX_FUEL[player] = MachineState.DEFAULT_MAX_FUEL;

                var stockpile = Stockpile.GetStockPile(player);

                while (stockpile.TryRemove(Mana.Item.ItemIndex) &&
                        machineState.Fuel < MachineState.MAX_FUEL[player])
                {
                    machineState.Fuel += 0.05f;
                }
                
                if (machineState.Fuel < MachineState.MAX_FUEL[player])
                    return Mana.Item.ItemIndex;
            }

            return GameLoader.Refuel_Icon;
        }

        public static void DoWork(Players.Player player, MachineState machineState)
        {
            if (machineState.Durability > 0 && 
                machineState.Fuel > 0 && 
                machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
            {
                machineState.Durability -= 0.01f;
                machineState.Fuel -= 0.05f;

                if (machineState.Durability < 0)
                    machineState.Durability = 0;

                if (machineState.Fuel <= 0)
                    machineState.Fuel = 0;

                machineState.NextTimeForWork = machineState.MachineSettings.WorkTime + Time.SecondsSinceStartDouble;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "TeleportPadMachineAudio", new List<string>() { GameLoader.AUDIO_FOLDER_PANDA + "/TeleportPadMachine.ogg" });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.RegisterTeleportPad")]
        public static void RegisterTeleportPad()
        {
            var rivets = new InventoryItem(BuiltinBlocks.IronRivet, 6);
            var steel = new InventoryItem(BuiltinBlocks.SteelIngot, 5);
            var sbb = new InventoryItem(BuiltinBlocks.ScienceBagBasic, 20);
            var sbc = new InventoryItem(BuiltinBlocks.ScienceBagColony, 20);
            var sba = new InventoryItem(BuiltinBlocks.ScienceBagAdvanced, 20);
            var crystal = new InventoryItem(BuiltinBlocks.Crystal, 5);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 50);
            var mana = new InventoryItem(Mana.Item.ItemIndex, 100);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { crystal, steel, rivets, sbb, sbc, sba, crystal, stone },
                                    new InventoryItem(Item.ItemIndex),
                                    6);

            //ItemTypesServer.LoadSortOrder(Item.name, GameLoader.GetNextItemSortIndex());
            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var TeleportPadTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            TeleportPadTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/TeleportPad.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".TeleportPad", TeleportPadTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.AddTeleportPad"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddTeleportPad(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var TeleportPadName = GameLoader.NAMESPACE + ".TeleportPad";
            var TeleportPadFlagNode = new JSONNode();
            TeleportPadFlagNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/TeleportPad.png");
            TeleportPadFlagNode["isPlaceable"] = new JSONNode(true);
            TeleportPadFlagNode.SetAs("onRemoveAmount", 1);
            TeleportPadFlagNode.SetAs("onPlaceAudio", "stonePlace");
            TeleportPadFlagNode.SetAs("onRemoveAudio", "stoneDelete");
            TeleportPadFlagNode.SetAs("isSolid", false);
            TeleportPadFlagNode.SetAs("sideall", "SELF");
            TeleportPadFlagNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/TeleportPad.obj");

            Item = new ItemTypesServer.ItemTypeRaw(TeleportPadName, TeleportPadFlagNode);
            items.Add(TeleportPadName, Item);
        }
    }
}
