using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Mods.APIProvider.Jobs;
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Jobs.Machinist.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "MachinistBenchTop.png";
            textureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "MachinistBenchTop.png";
            textureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "MachinistBenchTop.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "MachinistBenchTop", textureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Jobs.Machinist.AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            var item = new JSONNode()
              .SetAs("icon", GameLoader.ICON_PATH + "MachinistBench.png")
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("sideall", "stonebricks")
              .SetAs("sidey+", GameLoader.NAMESPACE + "MachinistBenchTop")
              .SetAs("npcLimit", 0);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("job"));
            item.SetAs("categories", categories);

            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, item));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Jobs.Machinist.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var gold = new InventoryItem(BuiltinBlocks.BronzeIngot, 2);
            var iron = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 4);

            var recipe = new Recipe(JOB_RECIPE,
                    new List<InventoryItem>() { gold, tools, stone, iron },
                    new InventoryItem(JOB_ITEM_KEY, 1), 2);

            //ItemTypesServer.LoadSortOrder(JOB_ITEM_KEY, GameLoader.GetNextItemSortIndex());
            RecipePlayer.AddOptionalRecipe(recipe);
            RecipeStorage.AddOptionalLimitTypeRecipe(Items.ItemFactory.JOB_CRAFTER, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".Jobs.Machinist.OnDeath")]
        public static void OnDeath(NPCBase nPC)
        {
            if (nPC.Job != null && nPC.Job.GetType() == typeof(MachinistJob) && ((MachinistJob)nPC.Job).TargetMachine != null)
            {
                ((MachinistJob)nPC.Job).TargetMachine.Machinist = null;
            }
        }
    }

    public class MachinistJob : BlockJobBase, IBlockJobBase, INPCTypeDefiner
    {
        const float COOLDOWN = 3f;
        protected float cooldown = 2f;
        public Items.Machines.MachineState TargetMachine { get; set; }
        public Items.Machines.MachineState PreviousMachine { get; set; }
        Vector3Int originalPosition;
        public static List<uint> OkStatus;
        private int _stuckCount = 0;
        public const string  MECHANICAL = "MECHANICAL";

        public virtual List<string> MachineTypes { get; set; } = new List<string>() { MECHANICAL };

        public override string NPCTypeKey
        {
            get
            {
                return GameLoader.NAMESPACE + ".Machinist";
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
            InitializeJob(player, (Vector3Int)node["position"], node.GetAs<int>("npcID"));
            return this;
        }

        public ITrackableBlock InitializeOnAdd(Vector3Int position, ushort type, Players.Player player)
        {
            originalPosition = position;
            InitializeJob(player, position, 0);
            return this;
        }

        public override JSONNode GetJSON()
        {
            return base.GetJSON().SetAs(nameof(originalPosition), (JSONNode)originalPosition);
        }

        public override Vector3Int GetJobLocation()
        {
            var pos = originalPosition;

            if (TargetMachine == null)
            {
                var ps = PlayerState.GetPlayerState(Owner);

                if (MachineManager.Machines.ContainsKey(owner))
                    foreach (var machine in MachineManager.Machines[Owner].Values.Where(m => m.Machinist == null && MachineTypes.Contains(m.MachineSettings.MachineType)))
                    {
                        if (machine != PreviousMachine && machine.PositionIsValid())
                        {
                            float dis = Vector3.Distance(machine.Position.Vector, pos.Vector);

                            if (dis <= 21)
                            {
                                if (machine.Durability < .5f ||
                                    machine.Fuel < .3f ||
                                    machine.Load < .3f)
                                {
                                    TargetMachine = machine;
                                    TargetMachine.Machinist = this;
                                    pos = TargetMachine.Position.GetClosestPositionWithinY(usedNPC.Position, 5);
                                    break;
                                }
                            }
                        }
                    }
            }
            else
            {
                pos = TargetMachine.Position.GetClosestPositionWithinY(usedNPC.Position, 3);
            }

            return pos;
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            ushort status = GameLoader.Waiting_Icon;
            float cooldown = COOLDOWN;
            bool fullyRepaired = false;

            if (TargetMachine != null && 
                usedNPC != null && 
                5 > Vector3.Distance(TargetMachine.Position.Vector, usedNPC.Position.Vector))
            {
                usedNPC.LookAt(TargetMachine.Position.Vector);

                if (TargetMachine.Durability < .50f)
                {
                    status = TargetMachine.MachineSettings.Repair(Owner, TargetMachine);
                    cooldown = TargetMachine.MachineSettings.RepairTime;
                    ServerManager.SendAudio(TargetMachine.Position.Vector, GameLoader.NAMESPACE + "HammerAudio");
                }
                else if (TargetMachine.Fuel < .3f)
                {
                    status = TargetMachine.MachineSettings.Refuel(Owner, TargetMachine);
                    cooldown = TargetMachine.MachineSettings.RefuelTime;
                    ServerManager.SendAudio(TargetMachine.Position.Vector, GameLoader.NAMESPACE + "ReloadingAudio");
                }
                else if (TargetMachine.Load < .3f)
                {
                    status = TargetMachine.MachineSettings.Reload(Owner, TargetMachine);
                    cooldown = TargetMachine.MachineSettings.ReloadTime;
                    ServerManager.SendAudio(TargetMachine.Position.Vector, GameLoader.NAMESPACE + "ReloadingAudio");
                }
                else
                {
                    PreviousMachine = null;
                    TargetMachine.Machinist = null;
                    TargetMachine = null;
                    fullyRepaired = true;
                }
            }

            // if the machine is gone, Abort.
            CheckIfValidMachine();

            if (OkStatus.Contains(status))
                _stuckCount = 0;
            else if (status != 0)
                _stuckCount++;

            if (_stuckCount > 5 || TargetMachine == null)
            {
                state.JobIsDone = true;
                status = GameLoader.Waiting_Icon;

                if (fullyRepaired)
                    cooldown = 0.5f;

                if (_stuckCount > 5)
                {
                    PreviousMachine = TargetMachine;
                    TargetMachine.Machinist = null;
                    TargetMachine = null;
                }
            }

            if (OkStatus.Contains(status))
                state.SetIndicator(new Shared.IndicatorState(cooldown, status));
            else if (status != 0)
                state.SetIndicator(new Shared.IndicatorState(cooldown, status, true));
            else
                state.SetIndicator(new Shared.IndicatorState(cooldown, BuiltinBlocks.ErrorMissing));

            state.SetCooldown(cooldown);
        }

        private void CheckIfValidMachine()
        {
            if (TargetMachine != null && !TargetMachine.PositionIsValid())
            {
                TargetMachine.Machinist = null;
                TargetMachine = null;
            }
        }

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            return NPCBase.NPCGoal.Job;
        }

        NPCTypeStandardSettings INPCTypeDefiner.GetNPCTypeDefinition()
        {
            return new NPCTypeStandardSettings
            {
                keyName = this.NPCTypeKey,
                printName = "Machinist",
                maskColor1 = new Color32(242, 132, 29, 255),
                type = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            };
        }
    }
}
