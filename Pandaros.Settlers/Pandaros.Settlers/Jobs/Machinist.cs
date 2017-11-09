using BlockTypes.Builtin;
using NPC;
using Pipliz;
using Pipliz.APIProvider.Jobs;
using Pipliz.JSON;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class MachinistRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Machinist";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".MachinistBench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Machinist.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            BlockJobManagerTracker.Register<MachinistJob>(JOB_ITEM_KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Machinist.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/MachinistBenchTop.png";
            textureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/MachinistBenchTop.png";
            textureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/MachinistBenchTop.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "MachinistBenchTop", textureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Machinist.AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, new JSONNode()
              .SetAs("icon", Path.Combine(GameLoader.ICON_FOLDER_PANDA, "MachinistBench.png"))
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("sideall", "stonebricks")
              .SetAs("sidey+", GameLoader.NAMESPACE + "MachinistBenchTop")
              .SetAs("npcLimit", 0)
            ));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Machinist.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var gold = new InventoryItem(BuiltinBlocks.GoldIngot, 2);
            var iron = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 4);

            var recipe = new Recipe(JOB_RECIPE,
                    new List<InventoryItem>() { gold, tools, stone, iron },
                    new InventoryItem(JOB_ITEM_KEY, 1), 2);

            RecipePlayer.AddOptionalRecipe(recipe);
            RecipeStorage.AddOptionalLimitTypeRecipe(Items.ItemFactory.JOB_CRAFTER, recipe);
        }
    }

    public class MachinistJob : BlockJobBase, IBlockJobBase, INPCTypeDefiner
    {
        protected float cooldown = 2f;
        Items.Machines.MachineState _targetMachine;
        Vector3Int originalPosition;

        public static List<Items.Machines.MachineState> _targetMachines = new List<Items.Machines.MachineState>();

        public static List<uint> OkStatus;

        public override string NPCTypeKey
        {
            get
            {
                return GameLoader.NAMESPACE + ".Machinist";
            }
        }

        public override float TimeBetweenJobs
        {
            get
            {
                return 2f;
            }
        }

        public override InventoryItem RecruitementItem
        {
            get
            {
                return new InventoryItem(BuiltinBlocks.CopperTools, 1);
            }
        }

        public override bool ToSleep => false;

        public override ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            originalPosition = (Vector3Int)node[nameof(originalPosition)];
            this.InitializeJob(player, (Vector3Int)node["position"], node.GetAs<int>("npcID"));
            return this;
        }

        public ITrackableBlock InitializeOnAdd(Vector3Int position, ushort type, Players.Player player)
        {
            originalPosition = position;
            this.InitializeJob(player, position, 0);
            return this;
        }

        public override JSONNode GetJSON()
        {
            return base.GetJSON().SetAs(nameof(originalPosition), (JSONNode)originalPosition);
        }

        public override Vector3Int GetJobLocation()
        {
            position = originalPosition;

            if (Items.Machines.MachineManager.Machines.ContainsKey(owner))
            foreach (var machine in Items.Machines.MachineManager.Machines[Owner])
            {
                float dis = Vector3.Distance(machine.Key.Vector, position.Vector);

                if (dis < 21)
                {
                        lock (_targetMachines)
                            if (!_targetMachines.Contains(machine.Value) &&
                                (machine.Value.Durability < .5f ||
                                machine.Value.Fuel < .3f ||
                                machine.Value.Load < .3f))
                            {
                                _targetMachine = machine.Value;
                                _targetMachines.Add(machine.Value);
                                position = Server.AI.AIManager.ClosestPosition(machine.Key, new Vector3Int(usedNPC.Position));
                                break;
                            }
                }
            }

            return position;
        }

        public override void OnNPCDoJob(ref NPCBase.NPCState state)
        {
            if (_targetMachine == null)
            {
                state.SetIndicator(NPCIndicatorType.Crafted, this.cooldown, GameLoader.Waiting_Icon);
                base.OverrideCooldown((double)this.cooldown);
                return;
            }

            if (3 > Vector3.Distance(_targetMachine.Position.Vector, usedNPC.Position))
            {
                ushort status = GameLoader.Waiting_Icon;
                float cooldown = 2f;

                usedNPC.LookAt(_targetMachine.Position.Vector);

                if (_targetMachine.Durability < .50f)
                {
                    status = _targetMachine.MachineSettings.Repair(Owner, _targetMachine);
                    cooldown = _targetMachine.MachineSettings.RepairTime;
                }
                else if (_targetMachine.Fuel < .3f)
                {
                    status = _targetMachine.MachineSettings.Refuel(Owner, _targetMachine);
                    cooldown = _targetMachine.MachineSettings.RefuelTime;
                }
                else if (_targetMachine.Load < .3f)
                {
                    status = _targetMachine.MachineSettings.Reload(Owner, _targetMachine);
                    cooldown = _targetMachine.MachineSettings.ReloadTime;
                }
                else
                {
                    lock (_targetMachines)
                    {
                        if (_targetMachines.Contains(_targetMachine))
                            _targetMachines.Remove(_targetMachine);

                        _targetMachine = null;
                    }
                }

                if (OkStatus.Contains(status) || _targetMachine == null)
                    state.SetIndicator(NPCIndicatorType.Crafted, cooldown, status);
                else
                    state.SetIndicator(NPCIndicatorType.MissingItem, _targetMachine.MachineSettings.RepairTime, status);

                base.OverrideCooldown(cooldown);
            }
            else
                base.OverrideCooldown(.5);
        }

        NPCTypeStandardSettings INPCTypeDefiner.GetNPCTypeDefinition()
        {
            return new NPCTypeStandardSettings
            {
                keyName = this.NPCTypeKey,
                printName = "Machinist",
                maskColor1 = new Color32(255, 211, 53, 255),
                type = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            };
        }
    }
}
