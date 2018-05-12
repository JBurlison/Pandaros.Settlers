using System.Collections.Generic;
using System.Linq;
using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using Shared;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManagerAttribute]
    public static class MachinistRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Machinist";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".MachinistBench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Machinist.RegisterJobs")]
        [ModLoader.ModCallbackProvidesForAttribute("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            BlockJobManagerTracker.Register<MachinistJob>(JOB_ITEM_KEY);
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterSelectedWorld,
            GameLoader.NAMESPACE + ".Jobs.Machinist.AddTextures")]
        [ModLoader.ModCallbackProvidesForAttribute("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "MachinistBenchTop.png";
            textureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "MachinistBenchTop.png";
            textureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "MachinistBenchTop.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "MachinistBenchTop", textureMapping);
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterAddingBaseTypes,
            GameLoader.NAMESPACE + ".Jobs.Machinist.AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            var item = new JSONNode()
                      .SetAs("icon", GameLoader.ICON_PATH + "MachinistBench.png")
                      .SetAs("onPlaceAudio", "stonePlace")
                      .SetAs("onRemoveAudio", "stoneDelete")
                      .SetAs("sideall", "stonebricks")
                      .SetAs("sidey+", GameLoader.NAMESPACE + "MachinistBenchTop")
                      .SetAs("npcLimit", 0);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("job"));
            item.SetAs("categories", categories);

            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, item));
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterWorldLoad,
            GameLoader.NAMESPACE + ".Jobs.Machinist.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var gold  = new InventoryItem(BuiltinBlocks.BronzeIngot, 2);
            var iron  = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 4);

            var recipe = new Recipe(JOB_RECIPE,
                                    new List<InventoryItem> {gold, tools, stone, iron},
                                    new InventoryItem(JOB_ITEM_KEY, 1), 2);

            RecipePlayer.AddOptionalRecipe(recipe);
            RecipeStorage.AddOptionalLimitTypeRecipe(ItemFactory.JOB_CRAFTER, recipe);
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnNPCDied,
            GameLoader.NAMESPACE + ".Jobs.Machinist.OnDeath")]
        public static void OnDeath(NPCBase nPC)
        {
            if (nPC.Job != null && nPC.Job.GetType() == typeof(MachinistJob) &&
                ((MachinistJob) nPC.Job).TargetMachine != null) ((MachinistJob) nPC.Job).TargetMachine.Machinist = null;
        }
    }

    public class MachinistJob : BlockJobBase, IBlockJobBase, INPCTypeDefiner
    {
        private const float COOLDOWN = 3f;
        public const string MECHANICAL = "MECHANICAL";
        public static List<uint> OkStatus;
        private int _stuckCount;
        protected float cooldown = 2f;
        private Vector3Int originalPosition;
        public MachineState TargetMachine { get; set; }
        public MachineState PreviousMachine { get; set; }

        public virtual List<string> MachineTypes { get; set; } = new List<string> {MECHANICAL};

        public override string NPCTypeKey => GameLoader.NAMESPACE + ".Machinist";

        public override InventoryItem RecruitementItem => new InventoryItem(BuiltinBlocks.CopperTools, 1);

        public override bool ToSleep => false;

        public ITrackableBlock InitializeOnAdd(Vector3Int position, ushort type, Players.Player player)
        {
            originalPosition = position;
            InitializeJob(player, position, 0);
            return this;
        }

        NPCTypeStandardSettings INPCTypeDefiner.GetNPCTypeDefinition()
        {
            return new NPCTypeStandardSettings
            {
                keyName           = NPCTypeKey,
                printName         = "Machinist",
                maskColor1        = new Color32(242, 132, 29, 255),
                type              = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            };
        }

        public override ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            originalPosition = (Vector3Int) node[nameof(originalPosition)];
            InitializeJob(player, (Vector3Int) node["position"], node.GetAs<int>("npcID"));
            return this;
        }

        public override JSONNode GetJSON()
        {
            return base.GetJSON().SetAs(nameof(originalPosition), (JSONNode) originalPosition);
        }

        public override Vector3Int GetJobLocation()
        {
            var pos = originalPosition;

            if (TargetMachine == null)
            {
                var ps = PlayerState.GetPlayerState(Owner);

                if (MachineManager.Machines.ContainsKey(owner))
                    foreach (var machine in MachineManager
                                           .Machines[Owner].Values
                                           .Where(m => m.Machinist == null &&
                                                       MachineTypes.Contains(m.MachineSettings.MachineType)))
                        if (machine != PreviousMachine && machine.PositionIsValid())
                        {
                            var dis = Vector3.Distance(machine.Position.Vector, pos.Vector);

                            if (dis <= 21)
                                if (machine.Durability < .5f ||
                                    machine.Fuel < .3f ||
                                    machine.Load < .3f)
                                {
                                    TargetMachine           = machine;
                                    TargetMachine.Machinist = this;

                                    pos =
                                        TargetMachine.Position.GetClosestPositionWithinY(usedNPC.Position, 5);

                                    break;
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
            var status        = GameLoader.Waiting_Icon;
            var cooldown      = COOLDOWN;
            var fullyRepaired = false;

            if (TargetMachine != null &&
                usedNPC != null &&
                5 > Vector3.Distance(TargetMachine.Position.Vector, usedNPC.Position.Vector))
            {
                usedNPC.LookAt(TargetMachine.Position.Vector);

                if (TargetMachine.Durability < .50f)
                {
                    status   = TargetMachine.MachineSettings.Repair(Owner, TargetMachine);
                    cooldown = TargetMachine.MachineSettings.RepairTime;
                    ServerManager.SendAudio(TargetMachine.Position.Vector, GameLoader.NAMESPACE + ".HammerAudio");
                }
                else if (TargetMachine.Fuel < .3f)
                {
                    status   = TargetMachine.MachineSettings.Refuel(Owner, TargetMachine);
                    cooldown = TargetMachine.MachineSettings.RefuelTime;
                    ServerManager.SendAudio(TargetMachine.Position.Vector, GameLoader.NAMESPACE + ".ReloadingAudio");
                }
                else if (TargetMachine.Load < .3f)
                {
                    status   = TargetMachine.MachineSettings.Reload(Owner, TargetMachine);
                    cooldown = TargetMachine.MachineSettings.ReloadTime;
                    ServerManager.SendAudio(TargetMachine.Position.Vector, GameLoader.NAMESPACE + ".ReloadingAudio");
                }
                else
                {
                    PreviousMachine         = null;
                    TargetMachine.Machinist = null;
                    TargetMachine           = null;
                    fullyRepaired           = true;
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
                status          = GameLoader.Waiting_Icon;

                if (fullyRepaired)
                    cooldown = 0.5f;

                if (_stuckCount > 5)
                {
                    PreviousMachine         = TargetMachine;
                    TargetMachine.Machinist = null;
                    TargetMachine           = null;
                }
            }

            if (OkStatus.Contains(status))
                state.SetIndicator(new IndicatorState(cooldown, status));
            else if (status != 0)
                state.SetIndicator(new IndicatorState(cooldown, status, true));
            else
                state.SetIndicator(new IndicatorState(cooldown, BuiltinBlocks.ErrorMissing));

            state.SetCooldown(cooldown);
        }

        private void CheckIfValidMachine()
        {
            if (TargetMachine != null && !TargetMachine.PositionIsValid())
            {
                TargetMachine.Machinist = null;
                TargetMachine           = null;
            }
        }

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            return NPCBase.NPCGoal.Job;
        }
    }
}